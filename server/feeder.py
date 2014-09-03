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
from pwd import getpwnam

import pymongo
import zmq
from daemon import runner

#Settings
endpoint_url = 'tcp://firehose.elite-market-data.net:9500' # EMDN subscriber endpoint URL
dbhost = 'localhost' # MongoDB server
dbport = 27017 # MongoDB port
max_data_length = 800 # Maximum character length of a received message
expected_version = '0.1' # The version of EMDN the script was written for

categories = {
    'foods': 'Foods',
    'textiles': 'Textiles',
    'industrial_materials': 'Industrial Materials',
    'chemicals': 'Chemicals',
    'medicines': 'Medicines',
    'drugs': 'Drugs',
    'machinery': 'Machinery',
    'technology': 'Technology',
    'consumer_items': 'Consumer Items',
    'waste': 'Waste',
    'metals': 'Metals',
    'minerals': 'Minerals',
    'weapons': 'Weapons'
}

items = {
    'grain': 'Grain',
    'animalmeat': 'Animal Meat',
    'fish': 'Fish',
    'foodcartridges': 'Food Cartridges',
    'syntheticmeat': 'Synthetic Meat',
    'tea': 'Tea',
    'coffee': 'Coffee',
    'leather': 'Leather',
    'naturalfabrics': 'Natural Fabrics',
    'syntheticfabrics': 'Synthetic Fabrics',
    'polymers': 'Polymers',
    'semiconductors': 'Semiconductors',
    'superconductors': 'Superconductors',
    'hydrogenfuel': 'Hydrogen Fuel',
    'performanceenhancers': 'Performance Enhancers',
    'basicmedicines': 'Basic Medicines',
    'beer': 'Beer',
    'mineralextractors': 'Mineral Extractors',
    'cropharvesters': 'Crop Harvesters',
    'hazardousenvironmentsuits': 'Hazardous Environment Suits',
    'robotics': 'Robotics',
    'autofabricators': 'Auto Fabricators',
    'domesticappliances': 'Domestic Appliances',
    'consumertechnology': 'Consumer Technology',
    'clothing': 'Clothing',
    'biowaste': 'Biowaste',
    'scrap': 'Scrap',
    'progenitorcells': 'Progenitor Cells',
    'gold': 'Gold',
    'beryllium': 'Beryllium',
    'indium': 'Indium',
    'gallium': 'Gallium',
    'tantalum': 'Tantalum',
    'uranium': 'Uranium',
    'lithium': 'Lithium',
    'titanium': 'Titanium',
    'copper': 'Copper',
    'aluminium': 'Aluminium',
    'algae': 'Algae',
    'fruitandvegetables': 'Fruits and Vegetables',
    'mineraloil': 'Mineral Oil',
    'pesticides': 'Pesticides',
    'agriculturalmedicines': 'Agricultural Medicines',
    'tobacco': 'Tobacco',
    'wine': 'Wine',
    'liquor': 'Liquor',
    'animalmonitors': 'Animal Monitors',
    'terrainenrichmentsystems': 'Terrain Enrichment Systems',
    'personalweapons': 'Personal Weapons',
    'heliostaticfurnaces': 'Heliostatic Furnaces',
    'marinesupplies': 'Marine Supplies',
    'computercomponents': 'Computer Components',
    'aquaponicsystems': 'Aquaponic Systems',
    'palladium': 'Palladium',
    'silver': 'Silver',
    'gallite': 'Gallite',
    'cobalt': 'Cobalt',
    'rutile': 'Rutile',
    'reactivearmour': 'Reactive Armour',
    'nonlethalweapons': 'Non-Lethal Weapons',
    'bertrandite': 'Bertrandite',
    'coltan': 'Coltan',
    'bauxite': 'Bauxite',
    'explosives': 'Explosives',
    'bioreducinglichen': 'Bio-Reducing Lichen',
    'indite': 'Indite',
    'lepidolite': 'Lepidolite',
    'uraninite': 'Uraninite',
    'advancedcatalysers': 'Advanced Catalysers',
    'combatstabilisers': 'Combat Stabilisers',
    'resonatingseparators': 'Resonating Separators',
    'basicnarcotics': 'Basic Narcotics'
}


class Feeder(object):
    def __init__(self):
        self.stdin_path = '/dev/null'
        self.stdout_path = '/dev/null'
        self.stderr_path = '/dev/tty'
        self.pidfile_path = '/var/run/feeder/feeder.pid'
        self.pidfile_timeout = 5
        
    def run(self):
        """ Main loop. Connects and receives the data """
        ctx = zmq.Context()
        logger.info('Starting')
        while(True):
            updates = ctx.socket(zmq.SUB)
            updates.linger = 0
            updates.setsockopt(zmq.SUBSCRIBE, '')
            try:
                updates.connect(endpoint_url)
                logger.info('Connected')
            
                while(True):
                    market_json = zlib.decompress(updates.recv())
                    Feeder.process(market_json)
            except Exception as ex:
                logger.warning('An error occured: {0}. Reconnecting in 10 seconds.', ex.message)
                logger.debug(traceback.format_exc())
            
            # Something went terribly wrong D:
            # Wait 10 seconds and try again
            time.sleep(10)

    @staticmethod
    def process(data):
        """ Saves the specified JSON into the database """
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
            logger.info('A frame contains an unexpected version ({0}).'.format(version))
            
        if 'type' not in pdata:
            notify_drapped_frame(data, 'it does not contain a type descriptor')
            return
            
        if pdata['type'] != 'marketquote':
            notify_dropped_frame(data, 'it has an unknown type ({0})'.format(type))
            return
        
        # Add display names
        if 'categoryName' in pdata['message']:
            if pdata['message']['categoryName'] in categories:
                pdata['message']['categoryDisplayName'] = categories[pdata['message']['categoryName']]
            else:
                logger.warning('Missing category display name: ' + pdata['message']['categoryName'])
                pdata['message']['categoryDisplayName'] = pdata['message']['categoryName']
        
        if 'itemName' in pdata['message']:
            if pdata['message']['itemName'] in items:
                pdata['message']['itemDisplayName'] = items[pdata['message']['itemName']]
            else
                logger.warning('Missing item display name: ' + pdata['message']['itemName']
                return # Drop unknown item names !
        else:
            logger.warning('Received an item without a name !')
            logger.debug('Frame contents: ' + data)
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
        logger.warning('A data frame was dropped because ' + reason)
        logger.debug('Frame contents: ' + data)

	def get_category_displayname(category):
		

app = Feeder()

logger = logging.getLogger('FeederDaemonLog')
logger.setLevel(logging.DEBUG)
formatter = logging.Formatter('%(asctime)s - %(name)s - %(levelname)s - %(message)s')
handler = logging.FileHandler('/var/log/feeder/feeder.log')
handler.setFormatter(formatter)
logger.addHandler(handler)

# Get the UIDs to run the daemon
uinfo = getpwnam('feeder')

runner = runner.DaemonRunner(app)
runner.daemon_context.files_preserve = [handler.stream]
runner.daemon_context.uid = uinfo.pw_uid
runner.daemon_context.gid = uinfo.pw_gid
runner.do_action()
