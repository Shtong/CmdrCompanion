#!/usr/bin/python
# -*- coding: utf-8 -*-
"""
This web application provides a REST endpoint to read the 
database maintained by the Feeder daemon
"""

import json

import pymongo
from bson import json_util
from werkzeug.wrappers import Request, Response

dbhost = 'localhost' # MongoDB server
dbport = 27017 # MongoDB port

@Request.application
def application(request):
    mongocx = pymongo.Connection(dbhost, dbport)
    cursor = mongocx.ccompanion.marketquote.find()
    data = [json.dumps(doc, default=json_util.default) for doc in cursor]
    return Response('[{0}]'.format(','.join(data)), mimetype='application/json')

if __name__ == '__main__':
    from werkzeug.serving import run_simple
    run_simple('0.0.0.0', 5000, application, use_debugger=True, use_reloader=True)