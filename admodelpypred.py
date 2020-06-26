# -*- coding: utf-8 -*-
"""
Created on Mon Feb 10 17:58:02 2020

@author: User
"""

import pyodbc 
import pandas as pd
import pickle
from sklearn.model_selection import train_test_split
from sklearn.preprocessing import StandardScaler
from sklearn.metrics import accuracy_score
from sklearn.linear_model import LinearRegression
from sklearn.linear_model import LogisticRegression
from sklearn.naive_bayes import GaussianNB
from sklearn.svm import SVC
from sklearn.neighbors import KNeighborsClassifier
from sklearn.tree import DecisionTreeClassifier
from sklearn.ensemble import RandomForestClassifier

conn = pyodbc.connect('Driver={SQL Server};'
                      'Server=DESKTOP-CCUEBU9;'
                      'Database=ADTeam6;'
                      'Trusted_Connection=yes;')
sql_query=pd.read_sql_query('''    select c.CategoryName,Datepart(month,rl.DateofSubmission) as Month,Datepart(year,rl.DateofSubmission) 
  as Year,sum(rd.RequisitionQuantity) as ReqQty,d.Departmentname
  from RequisitionDetail rd,RequisitionList rl,Category c,Stationery s,Department d
  where rd.RequisitionID=rl.RequisitionID and rd.ItemID=s.ItemID and s.CategoryID=c.CategoryID and rl.DeptID_FK=d.DepartmentID
  group by c.CategoryName,d.Departmentname,Datepart(month,rl.DateofSubmission),Datepart(year,rl.DateofSubmission)''',conn)


df = pd.DataFrame(sql_query, columns=['CategoryName','Departmentname','Month','Year','ReqQty'])
df.head(10)
df.fillna(df.mean())
dept={
    'English Dept':0,
    'Science Dept':1,
    
}
cat={
    'Clip':0,
    'Envelop':1,
    'Eraser':2,

    
}

df['Departmentname']=df['Departmentname'].map(dept)
df['CategoryName']=df['CategoryName'].map(cat)

df

x=df[['CategoryName','Departmentname','Month','Year']]
y=df['ReqQty']
x_train, x_test, y_train, y_test = train_test_split(x, y, random_state = 3)
x_train
x_test
y_train
y_test
log = LogisticRegression(solver = 'lbfgs')
lr=LinearRegression()



knn = KNeighborsClassifier(n_neighbors = 1) 
knn.fit(x_train, y_train)
y_pred = knn.predict(x_test)
accuracy_score(y_test, y_pred)
print(knn.predict([[0,0,3,2020]]))
with open('admodelfinal1','wb') as f:
    pickle.dump(dept, f)
    pickle.dump(cat, f)
    pickle.dump(knn,f)