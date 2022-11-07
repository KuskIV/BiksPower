import os
import re
from datetime import datetime,timedelta
import pandas as pd

path = "Measurements"
dir_list = os.listdir(path)
print(dir_list)

def GetParameter(fileName, key):
    x = re.search(key+"\d+", fileName)
    y = re.search("\d+", x[0])
    return int(y.group())

def GetFinalTimeStamp(file):
    x = re.search("(\d*-\d*){6}", file)
    dateStr = x[0][1:]
    date = datetime.strptime(dateStr, "%y-%m-%d-%H-%M-%S-%f")
    #print(date2.strftime("%y-%m-%d-%H-%M-%S-%f"))
    return date

def GetTimeStampIncrement(date, increment):
    return date + timedelta(milliseconds=increment)

def AddTimeStamps(df,date,frequency):
    dates = [date]
    for i in range(1,len(df)):
        dates.append(GetTimeStampIncrement(dates[-1],-frequency*1000))
    strDates = [x.strftime("%y-%m-%d-%H-%M-%S-%f") for x in dates]
    df['TimeStamp'] = strDates

def AddPowers(df,key, frequency):
    lst = df[key]
    watt = [key]
    for i in range(0,len(lst)-1):
        watt.append(lst[i]*10*230*frequency)
    df[key+"Power(joule)"] = watt

for file in dir_list:
    Samples = GetParameter(file, "Samples")
    History = GetParameter(file, "History")
    Date = GetFinalTimeStamp(file)
    frequency = History/Samples
    df = pd.read_csv(path+"\\"+file)
    AddTimeStamps(df, Date, frequency)
    AddPowers(df,'C1TrueRMS ', frequency)
    AddPowers(df, 'C1ACRMS', frequency)
    print(df)
