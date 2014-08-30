#!/usr/bin/python
# -*- coding: utf-8 -*-

"""
This module connects to the EMDN as a subscriber and writes all the changes in the MongoDB database.
"""

import logging
import simplejson
import time
import traceback
import zlib
from daemon import runner

import pymongo
import zmq

#Settings
endpoint_url = 'tcp://firehose.elite-market-data.net:9500' # EMDN subscriber endpoint URL
dbhost = 'localhost' # MongoDB server
dbport = 27017 # MongoDB port
max_data_length = 800 # Maximum character length of a received message
expected_version = '0.1' # The version of EMDN the script was written for

class Feeder(object):
    def __init__(self):
        self.stdin_path = '/dev/null'
        self.stdout_path = '/dev/null'
        self.stderr_path = '/dev/tty'
        self.pidfile_path = '/var/run/feeder.pid'
        self.pidfile_timeout = 5
        
    def run(self):
        """ Main loop. Connects and receives the data """
        ctx = zmq.Context()
        logging.info('Starting')
        while(True):
            updates = ctx.socket(zmq.SUB)
            updates.linger = 0
            updates.setsockopt(zmq.SUBSCRIBE, '')
            try:
                updates.connect(endpoint_url)
                logging.info('Connected')
            
                while(True):
                    market_json = zlib.decompress(updates.recv())
                    Feeder.process(market_json)
            except Exception as ex:
                logging.warning('An error occured: {0}. Reconnecting in 10 seconds.', ex.message)
                logging.debug(traceback.format_exc())
            
            # Something went terribly wrong D:
            # Wait 10 seconds and try again
            time.sleep(10)

    @staticmethod
    def process(data):
        """ Saves the specified JSON into the database """
        logging.debug('Received: ' + data)

        if len(data) > max_data_length:
            notify_dropped_frame(data, 'it is suspiciously long!')
            return

        try:
            pdata = simplejson.loads(data)
        except simplejson.JSONDecodeError as ex:
            notify_dropped_frame(data, 'it could not be parsed by simplejson')
            return
        
        if 'version' not in pdata:
            notify_dropped_frame(data, 'it does not contain a version number')
            return
        
        if pdata['version'] != expected_version:
            logging.info('A frame contains an unexpected version ({0}).'.format(version))
            
        if 'type' not in pdata:
            notify_drapped_frame(data, 'it does not contain a type descriptor')
            return
            
        if pdata['type'] != 'marketquote':
            notify_dropped_frame(data, 'it has an unknown type ({0})'.format(type))
            return
        
        # Insert the data
        mongo = pymongo.Connection(dbhost, dbport)
        db = mongo.ccompanion
        table = db.marketquote
        table.ensure_index([('stationName', 1), ('itemName', 1)]) # TODO : Move this to do it only once ?
        
        table.update({'stationName': pdata['message']['stationName'], 'itemName': pdata['message']['itemName']},
                     pdata['message'],
                     upsert=True)
                     
    @staticmethod
    def notify_dropped_frame(data, reason):
        logging.warning('A data frame was dropped because ' + reason)
        logging.debug('Frame contents: ' + data)


app = Feeder()

logger = logging.getLogger('FeederDaemonLog')
logger.setLevel(logging.DEBUG)
formatter = logging.Formatter('%(asctime)s - %(name)s - %(levelname)s - %(message)s')
handler = logging.FileHandler('/var/log/feeder/feeder.log')
handler.setFormatter(formatter)
logger.addHandler(handler)

runner = runner.DaemonRunner(app)
runner.daemon_context.files_preserve = [handler.stream]
runner.do_action()
