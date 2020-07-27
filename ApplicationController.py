# -*- coding: utf-8 -*-
"""
    Created on Wed Jun  3 16:52:46 2020
    
    @author: Ghercioglo "Romeon0" Roman
"""


import tensorflow as tf
#tf.enable_eager_execution() #INFO: for charPath.numpy()
import IPython.display as display #show images
import matplotlib.pyplot as plt
import numpy as np
import random
import pathlib
import ApplicationModel as network #INFO: Neural network
from tensorflow import keras
from PIL import Image 
import cv2 as cv
from matplotlib import pyplot as plt
import operator
import threading
import os
import datetime
import stat


#GLOBAL SETTINGS ----------
needTrain = False
needRecognize = False
needDecupate = False
needTestAccuracy = False
consoleMode = False
RANDOM_SEED = 4234233


#tf.set_random_seed(RANDOM_SEED)
random.seed(RANDOM_SEED)
np.random.seed(RANDOM_SEED)


print("Is GPU available? ", tf.test.is_gpu_available(cuda_only=True))
#------------------------




#INFO: Functions ----------

def HasHiddenAttribute(filepath):
    return bool(os.stat(filepath).st_file_attributes & stat.FILE_ATTRIBUTE_HIDDEN)



def ShowGrayImage(pixels, size = None, msg = None):
    if consoleMode == True:
        if size is None:
            plt.figure()
        else:
            plt.figure(figsize=size)
        plt.imshow(pixels, cmap='gray')
        if msg is not None:
            plt.title(msg)
        plt.xticks([]),plt.yticks([])
        plt.colorbar()
        plt.grid(False)
        plt.show()  
        
        
def ShowImage(pixels, size = None, msg = None):
    if consoleMode == True:
        if size is None:
            plt.figure()
        else:
            plt.figure(figsize=size)
        plt.imshow(pixels)
        if msg is not None:
            plt.title(msg)
        plt.xticks([]),plt.yticks([])
        plt.colorbar()
        plt.grid(False)
        plt.show()  


def ShowSubplotGray(images, size = None):
    if consoleMode == True:
        if size is None:
            plt.figure()
        else:
            plt.figure(figsize=size)
        cols = len(images) / 2+1
        rows = cols
        if cols < 1:
            rows = cols = 1
        for i in range(0, len(images), 1):
            ax= plt.subplot(rows,cols ,i+1)
            im=plt.imshow(images[i],'gray')
            plt.title(i)

def LoadData(path, class_names, labelsOffset=0):
    #INFO: Some global vars ----------
    data = np.zeros((2000,28,28))
    labels = np.zeros((2000, 1))

    
    #INFO: Read and preprocess data  ----------
    counter = 0
    dataLen = 0
    datasetRoot = pathlib.Path(path)
    tempData = []
    #allowedChars = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9']
    for charsFolder in datasetRoot.glob("*"):
      if os.path.isdir(path + charsFolder.name)==False:
          continue;
          
      charsPath = path + charsFolder.name + "/"
     # files = tf.data.Dataset.list_files(charsPath)
      files = os.listdir(charsPath)
      
      #print("Files count: ", len(list(files)))
      if True:#(charsFolder.name) != ("Numbers"):# and allowedChars.count(charsFolder.name) > 0:     
          for charPath in files: #.take(1):
            dataLen+=1
            if len(data) <= dataLen:
                data = np.resize(data, (len(data)+500,28,28))   
            
            filepath = charsPath + charPath
             
            #INFO: Read image
            if HasHiddenAttribute(filepath) == True:
                print(filepath + " is hidden file.")
                continue     
            image = Image.open(filepath)
            image = image.resize(size=(28, 28))
            #INFO: rgb to grayscale (60,60,3)->(60,60,1)
            pixels = image.convert("L")
            #INFO: Image->matrix 60x60
            pixels = np.asarray(pixels)
            #INFO: Create label            
            label = ord(charsFolder.name) - ord(class_names[0]) + labelsOffset
            #INFO: Save data (typle: label, image pixels)
            tempData.append((label, pixels))
            counter = 1 + counter
    data = np.resize(data, (dataLen,28,28))      
    labels = np.resize(labels, (dataLen,1))    
    
    #INFO: Shuffling temp data
    random.shuffle(tempData)
    random.shuffle(tempData)
    random.shuffle(tempData)
    random.shuffle(tempData)
    random.shuffle(tempData)
    #INFO: Assign shuffled temp data to our data and labels
    counter = 0
    for value in tempData:
        data[counter] = value[1]
        labels[counter] = value[0]
        counter = counter + 1
        
    return data, labels
    

def LoadLettersData():
    path = "./Data/Letters/"
    class_names = ['A', 'B', 'C', 'D', 'E', 
                   'F','G', 'H', 'I','J',
                   'K', 'L','M', 'N', 'O',
                   'P','Q','R','S','T','U',
                   'V','W','X','Y','Z']
    return LoadData(path, class_names)


def LoadNumbersData():
    path = "./Data/Numbers/"
    class_names = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9']
    return LoadData(path, class_names, 26)
        

def FindAllPlates(image):
    img = cv.cvtColor(image, cv.COLOR_BGR2GRAY) 
    img = cv.resize(img, (500,400), interpolation = cv.INTER_AREA)   
    #
    _,filtered = cv.threshold(img,127,255,0)
    #
    contours, hierarchy = cv.findContours(filtered, cv.RETR_TREE, cv.CHAIN_APPROX_SIMPLE)
    contours = sorted(contours, key = cv.contourArea, reverse = True)
    print("Contours found: ", len(contours))
    #
    counter = 0
    platePoints = []
    for contour in contours[0:10]:
        # approximate the contour
        peri = cv.arcLength(contour, True)
        points = cv.approxPolyDP(contour, 0.018 * peri, True)
      
        #INFO: If 4 points -> assume is rectangle
        if len(points) == 4:
            foundContour = contours[counter]
            platePoints.append(points)  
            x, y, w, h = cv.boundingRect(points)
            #print("Found contour with 4 points. Size: ", w, h, "; Area: ", cv.contourArea(foundContour))  
            #cv.drawContours(img, [foundContour], 0, (0,255,0), 3)
           # break
        counter = counter + 1
    
    if len(platePoints) != 0:  
        counter = 0
        for index in range(0, len(platePoints), 1):
            points = platePoints[index]
            x, y, w, h = cv.boundingRect(points)
            points = filtered[y:y+h, x:x+w]
            platePoints[index] = cv.resize(points, (200,100), interpolation = cv.INTER_AREA)
        return platePoints
    
    return []

   
def FindPlate(image):        
    img = cv.cvtColor(image, cv.COLOR_BGR2GRAY) 
    #
    _,filtered = cv.threshold(img,127,255,0)
    #
    ShowGrayImage(filtered, size = (10,10), msg = "Image")

    contours, hierarchy = cv.findContours(filtered, cv.RETR_TREE, cv.CHAIN_APPROX_SIMPLE)
    contours = sorted(contours, key = cv.contourArea, reverse = True)
    #print("Contours found: ", len(contours))
    #
    counter = 0
    platePoints = None
    for contour in contours[0:10]:
        # approximate the contour
        peri = cv.arcLength(contour, True)
        points = cv.approxPolyDP(contour, 0.018 * peri, True)
      
        #INFO: If 4 points -> assume is rectangle
        #print("(plate) Contour points: ", len(points) )  
        if len(points) == 4:
            foundContour = contours[counter]
            platePoints = points  
            x, y, w, h = cv.boundingRect(platePoints)
            #print("Found contour with 4 points. Size: ", w, h, "; Area: ", cv.contourArea(foundContour))  
           # cv.drawContours(image, [foundContour], 0, (140,0,0), 10)
            break
        counter = counter + 1
    
    ##Show drawn contours
    # plt.figure(figsize=(20,10))
    # plt.imshow(image, 'gray')
    # plt.title("Plate Contours")
    # plt.xticks([]),plt.yticks([])
    
    if platePoints is not None:  
        x, y, w, h = cv.boundingRect(platePoints)
        colored = image[y:y+h, x:x+w]
        binarized = filtered[y:y+h, x:x+w]
        binarized = cv.resize(binarized, (200,100), interpolation = cv.INTER_AREA)
             
        ShowGrayImage(binarized, (10,5))
        
        return colored, binarized
    
    return None, None


#INFO: Check if a rectangle inside another rectangle. 
#      Input: two tuples with format (xPos, yPos, width, height)
def IsRectangleInRectangle(r1, r2):
    a1 = r1[0] #r1.x1
    b1 = r1[1] #r1.y1
    a2 = a1+r1[2] #r1.x2
    b2 = b1+r1[3] #r1.y2
    c1 = r2[0] #r2.x1
    d1 = r2[0] #r2.y1
    c2 = c1+r2[2] #r2.x2
    d2 = c2+r2[3] #r2.y2
    #return r1.x1 < r2.x1 < r2.x2 < r1.x2 and r1.y1 < r2.y1 < r2.y2 < r1.y2
    return a1 < c1 < c2 < a2 and b1 < d1 < d2 < b2

def FindCharacters(image):
    originalImg = image
    
    chars_mask = np.zeros((image.shape[0],image.shape[1],3), np.uint8) 
    
    if consoleMode:
        _, contours, hierarchy = cv.findContours(image, cv.RETR_TREE, cv.CHAIN_APPROX_SIMPLE)
    else:
        contours, hierarchy = cv.findContours(image, cv.RETR_TREE, cv.CHAIN_APPROX_SIMPLE)
    contours = sorted(contours, key = cv.contourArea, reverse = True)
    #print("Contours found: ", len(contours))
    #
    
    #INFO: Make mask of characters
    for contour in contours[0:20]:
        # approximate the contour
        peri = cv.arcLength(contour, True)
        points = cv.approxPolyDP(contour, 0.018 * peri, True)
      
        x,y,w,h = cv.boundingRect(points)
        area = cv.contourArea(contour)
        #print("(chars) Contour Size: ", w, h)
       
        if h >= 20 and h < 90 and w >= 10 and w <= 25:
            cv.rectangle(chars_mask, (x, y), (x+w-1,y+h-1), (255,255,255), thickness=cv.FILLED)
            #print("Character found.")
    chars_mask = cv.cvtColor(chars_mask, cv.COLOR_BGR2GRAY)
     
    #INFO: For each contour found in mask, save character
    characters = {}
    contours, hierarchy = cv.findContours(chars_mask, cv.RETR_TREE, cv.CHAIN_APPROX_SIMPLE)
    for contour in contours[0:20]:
        # approximate the contour
        peri = cv.arcLength(contour, True)
        points = cv.approxPolyDP(contour, 0.018 * peri, True)
      
        x,y,w,h = cv.boundingRect(points)
        charImage = originalImg[y:y+h, x:x+w]
        characters[x] = (x,charImage)
    
    
    ShowImage(chars_mask, (10,10), "Char contours")

    result = []
    for i in sorted(characters, key=lambda k: k ):
        result.append(characters[i][1])       
    return result


#INFO: Get all plates from each image, 
#      get characters from each plate and save each character
def DecupateLettersFromImages():
    root = "./Data/Plates/"
    out_root = "./Data/Plates/Chars/"
    counter = 1
    for i in range(28, 36): 
        path = root + "plate" + str(i) + ".jpg"
        image = cv.imread(path) 
        plates = FindAllPlates(image)
        for plate in plates:
            characters = FindCharacters(plate)
            foundCharsCount = len(characters)
           # print("Found ",foundCharsCount," characters.")
            if foundCharsCount!=0:       
                for char in characters:
                    char = cv.resize(char, (60,60), interpolation = cv.INTER_AREA)
                    char_normed = 255 * (char - char.min()) / (char.max() - char.min())
                    char_normed = np.array(char_normed, np.int)
                    
                    cv.imwrite(out_root + str(counter) + ".png", char_normed)
                    counter = counter + 1

#INFO: Search plate in image && characters on plate. 
#      Returns plate(image), characters(image), found number(string)
model = None
def RecognizeNumber(path, model = None):        
    image = cv.imread(path)  
    
    coloredPlate, binaryPlate = FindPlate(image)
    
    if binaryPlate is None:  
        #print("Plate not found!")
        a = 1
    else:   
        #print("Plate found. Searching characters...")
        
        #INFO: Search characters
        characters = FindCharacters(binaryPlate)
        foundCharsCount = len(characters)
        #print("Found ",foundCharsCount," characters.")
        
        if foundCharsCount!=0:
            normalized_characters = characters[:]
            for i in range(0, len(normalized_characters), 1):
                normalized_characters[i] = normalized_characters[i] / 255
            
            #INFO: Show found characters
            ShowSubplotGray(normalized_characters, (10, 10))
            
            #INFO: Create data
            data = np.zeros((len(normalized_characters), 28, 28))
            for index in range(0, len(normalized_characters), 1):
                #Resize to 28x28
                char = cv.resize(normalized_characters[index], (28,28), interpolation = cv.INTER_AREA)
                #Get pixels
                rows, cols = char.shape    
                pixels = char[0:rows, 0:cols]
                #Add to data
                data[index,:,:] = pixels
            
            #INFO: Predict
            if model is None:
                print("model is None! Loading the best one...")
                model = network.Load("./Data/Networks/NNModel_Both_best.network")
            #print("Found: ")
            answer = network.Predict(model, data)
            #print("Answer: ", ''.join(answer))
            return binaryPlate, characters, ''.join(answer)
        return binaryPlate, None, None
    return None, None, None



def Train(epochEndCallback, maxEpochs, model=None):
    if model is None:
        model = network.Create()
  
    def TrainFunc(arg):  
        
        print("Loading data: started.")
        #Load data
        data, labels = LoadNumbersData()
        data2, labels2 = LoadLettersData()  
        dataLen = len(data) 
        
        #'''
        #INFO: Concatenate all data(letters+digits) and shuffle
        data = np.concatenate((data, data2))
        labels = np.concatenate((labels, labels2))
        dataLen = len(data) 
        print("Loading data: found " + str(dataLen) + " characters.")
        temp = []
        counter = 0
        for i in range(0, dataLen, 1):
            temp.append((data[counter], labels[counter]))
            counter = counter + 1
        random.shuffle(temp)  
        random.shuffle(temp)  
        random.shuffle(temp)  
        random.shuffle(temp)  
        random.shuffle(temp)  
        #INFO: Assign shuffled data to our data and labels
        counter = 0
        for i in range(0, dataLen, 1):
            data[counter]= temp[counter][0]
            labels[counter] = temp[counter][1]
            counter = counter + 1
        #INFO: Divide data to train set and test set
        train_images = data[0:int(dataLen*0.8)]
        train_labels = labels[0:int(dataLen*0.8)]
        test_images = data[int(dataLen*0.8):dataLen]
        test_labels = labels[int(dataLen*0.8):dataLen]
        # print("Total Images count: ", len(data))
        # print("train_images count: ", len(train_images))
        # print("train_labels count: ", len(train_labels))
        # print("test_images count: ", len(test_images))
        # print("test_labels count: ", len(test_labels))
        print("Loading data: finished.")

        ## INFO: Train 5 networks and select the best one
        # bestNetwork = 0
        # bestTestError = -999999
        # print("Train Network: Started.")
        # for i in range(1, 5, 1):
        #     #Create and train
        #     model = network.Create()
        #     network.Train(model, train_images, train_labels)
        #     test_acc = network.Test(model, test_images, test_labels)
        #     network.Predict_test(model, test_images, test_labels)
        #     network.Save(model, "./Data/Networks/temp/NNModel_Both_"+str(i)+".network")
        #     print("Network", i, "acc: ", test_acc)
        #     if bestTestError < test_acc:
        #         bestNetwork = i
        #         bestTestError = test_acc
        #     #------------------
        # print("Best Network ", bestNetwork, " with accuracy  ", bestTestError)
        
        print("Train Network: Started.")
        network.Train(model, train_images, train_labels, epochEndCallback, maxEpochs)
        test_acc = network.Test(model, test_images, test_labels)
        network.Predict_test(model, test_images, test_labels)
        print("Train Network: Finished.")
        
        
            
    thread = threading.Thread(target=TrainFunc, args=(1,))
    thread.start()
    return model
    

def StopTrain(model):
    network.StopTrain(model)
    
    
def LoadNetwork(path):
    return network.Load(path)

def SaveNetwork(model, path):
    return network.Save(model, path)
    
def CreateNewNetwork():
    return network.Create()






#"""
#INFO: Load data & train network ------------------
if needTrain == True:
    print("Loading data: started.")
    #Load data
    data, labels = LoadNumbersData()
    data2, labels2 = LoadLettersData()
    
    dataLen = len(data) 
    #'''
    #INFO: Concatenate all data(letters+digits) and shuffle
    data = np.concatenate((data, data2))
    labels = np.concatenate((labels, labels2))
    dataLen = len(data) 
    temp = []
    counter = 0
    for i in range(0, dataLen, 1):
        temp.append((data[counter], labels[counter]))
        counter = counter + 1

    random.shuffle(temp)
    random.shuffle(temp)  
    counter = 0
    for i in range(0, dataLen, 1):
        data[counter]= temp[counter][0]
        labels[counter] = temp[counter][1]
        counter = counter + 1
    train_images = data[0:int(dataLen*0.8)]
    train_labels = labels[0:int(dataLen*0.8)]
    test_images = data[int(dataLen*0.8):dataLen]
    test_labels = labels[int(dataLen*0.8):dataLen]
    print("Loading data: finished.")
    
    print("Training: started.")
    print("Training: loaded " + str(dataLen) + " characters.")
    model = network.Create()    
    network.Train(model,train_images, train_labels, maxEpochs=30)
    print("Training: finished.")



#----Find plate, letters & recognize -----------------------
if needRecognize == True: 
    path = "./Data/Cars/car (7).jpg"
    RecognizeNumber(path, model)  
    
    ## ----------- Tests -----------
    # model = network.Load("./Data/Networks/NNModel_Both_best.network")
    # averages = np.zeros((13))  
    # for nrTest in range(1, 10, 1):
    #     totalTime = 0
    #     for nrImg in range(1, 13, 1):
    #         path = "./Data/Cars/car ("+str(nrImg)+").jpg"
    #         time1 = datetime.datetime.now()
    #         binaryPlate, characters, answer = RecognizeNumber(path, model)  
    #         time2 = datetime.datetime.now()
    #         diff = (time2-time1)
    #         s = diff.seconds
    #         ms = diff.microseconds/ 1000
    #         found = str(binaryPlate is not None) + ";"+str(characters is not None)+";"+str(answer is not None)
    #         print("Time elapsed("+str(nrImg-1)+"): " + str(ms) + "ms.", "Found: "+found) 
    #         totalTime += diff.microseconds / 1000
    #         averages[nrImg-1] += ms
    #     print("Average time elapsed: ", (totalTime / 12) ,"ms")  
    #     averages[12]+=(totalTime / 12)
    # for nrImg in range(1, 13, 1):
    #     print("Avg["+str(nrImg)+"] time elapsed: ", (averages[nrImg-1] / 12) ,"ms")  
    # print("Total Avg time elapsed: ", (averages[12]/10)) 
    
    # for nrImg in range(1, 13, 1):
    #        path = "./Data/Cars/car ("+str(nrImg)+").jpg"
    #        time1 = datetime.datetime.now()
    #        binaryPlate, characters, answer = RecognizeNumber(path, model)  
    #        time2 = datetime.datetime.now()
    #        diff = (time2-time1)
    #        s = diff.seconds
    #        ms = diff.microseconds/ 1000
    #        found = str(binaryPlate is not None)
    #        print(str(nrImg-1)+". Plate found: " + found + ".", "Answer: " + str(answer)) 
    #        totalTime += diff.microseconds / 1000
    #        averages[nrImg-1] += ms
    ## ----------------------
#---------------------------
    
#----Cut characters from images -----------------------
if needDecupate == True:
    DecupateLettersFromImages()         
#--------------------------- 
    
    
#----Test loaded network accuracy -----------------------
if needTestAccuracy:
        #Load data
        data, labels = LoadNumbersData()
        data2, labels2 = LoadLettersData()
        
        dataLen = len(data) 
        #'''
        #INFO: Concatenate all data(letters+digits) and shuffle
        data = np.concatenate((data, data2))
        labels = np.concatenate((labels, labels2))
        dataLen = len(data) 
        temp = []
        counter = 0
        for i in range(0, dataLen, 1):
            temp.append((data[counter], labels[counter]))
            counter = counter + 1

        random.shuffle(temp)
        random.shuffle(temp)  
        random.shuffle(temp)  
        random.shuffle(temp)  
        random.shuffle(temp)  
        counter = 0
        for i in range(0, dataLen, 1):
            data[counter]= temp[counter][0]
            labels[counter] = temp[counter][1]
            counter = counter + 1
            if i < 5:
                print("data ", i, ": ", labels[counter])
        #'''
        
        train_images = data[0:int(dataLen*0.8)]
        train_labels = labels[0:int(dataLen*0.8)]
        test_images = data[int(dataLen*0.8):dataLen]
        test_labels = labels[int(dataLen*0.8):dataLen]
    
        
        model = network.Load("./Data/Networks/NNModel_Both_best.network")    
        
        test_acc = network.Test(model, test_images, test_labels)
        network.Predict_test(model, test_images, test_labels)
        print("Network test accuracy: ", test_acc)
        
        
        
def SaveRectangles(image):
    img = cv.cvtColor(image, cv.COLOR_BGR2GRAY) 
    img = cv.resize(img, (500,400), interpolation = cv.INTER_AREA)   
    #
    _,filtered = cv.threshold(img,127,255,0)
    ShowGrayImage(filtered,size=(30,30))
    #
    if consoleMode:
        _,contours, hierarchy = cv.findContours(filtered, cv.RETR_TREE, cv.CHAIN_APPROX_SIMPLE)
    else:
        contours, hierarchy = cv.findContours(filtered, cv.RETR_TREE, cv.CHAIN_APPROX_SIMPLE)
    contours = sorted(contours, key = cv.contourArea, reverse = True)
    print("Contours found: ", len(contours))
    #
    counter = 0
    platePoints = []
    newImg = np.zeros(image.shape)
    for contour in contours:#[0:1000]:
        # approximate the contour
        peri = cv.arcLength(contour, True)
        points = cv.approxPolyDP(contour, 0.018 * peri, True)
      
        #INFO: If 4 points -> assume is rectangle
        if len(points) == 4:
            foundContour = contours[counter]
            platePoints.append(points)  
            x, y, w, h = cv.boundingRect(points)
           # print("Found contour with 4 points. Size: ", w, h, "; Area: ", cv.contourArea(foundContour))  
            cv.drawContours(img, [foundContour], 0, (0,255,0), 3)
            
            newImg[x:x+w,y:y+h,:] = image[x:x+w,y:y+h,:]
           # break
        counter = counter + 1
    cv.imwrite("flags_selected.png", newImg)
SaveRectangles(cv.imread(".\World_Flags.jpg"))

        
