# -*- coding: utf-8 -*-
"""
Created on Mon Feb 10 18:09:43 2020

@author: User
"""

import pandas as pd
from flask import Flask, request, jsonify
import pickle
from waitress import serve

app = Flask(__name__)

with open('admodelfinal1', 'rb') as f:
    dept=pickle.load(f)
    cat=pickle.load(f)
    stock_knn=pickle.load(f)
    print(dept ,"Category", cat)
    print(stock_knn) 
    
@app.route('/', methods=['POST','GET'])
def api():
    
    req = request.get_json(force = True) 
    CategoryName = cat.get(req['CategoryName'])
    print(req['CategoryName'])  
    print(CategoryName)
    Departmentname = dept.get(req['Departmentname'])
    print(req['Departmentname'])    
    print(Departmentname)
    print(req['Month'])
    print(req['Year'])
    prediction = stock_knn.predict([[CategoryName,Departmentname, req['Month'],req['Year']]])
    print(prediction)
    result = prediction.tolist()[0]
    print(result)
    return jsonify(result)

# run the server
if __name__ == '__main__':
   
    serve(app, host="0.0.0.0", port=5000)
    app.run('127.0.0.1', 5000,debug=True)
    
 