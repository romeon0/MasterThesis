# -*- coding: utf-8 -*-
"""
    Created on Wed Jun  1 14:00:46 2020
    
    @author: Ghercioglo "Romeon0" Roman
"""

# TensorFlow and Keras
import tensorflow as tf
from tensorflow import keras
# Helper libraries
import numpy as np
import matplotlib.pyplot as plt
from tensorflow.keras.callbacks import Callback

RANDOM_SEED = 4234233
np.random.seed(RANDOM_SEED)



#Functions ----------------------------------------
def Hello():
    print("Hello from ApplicationModel.py script!")

def Create():
    #Create model
    model = keras.Sequential([
        keras.layers.Conv1D(input_shape=(28, 28), filters=4, kernel_size=3),
        tf.keras.layers.AveragePooling1D(pool_size=2,input_shape=(26, 26)),
        keras.layers.Flatten(),
        keras.layers.Dense(100, activation='tanh'),
        keras.layers.Dense(60, activation='tanh'),
        keras.layers.Dense(26+10)
    ])  
    
    #Compile the model
    model.compile(optimizer='adam',
                  loss=tf.keras.losses.SparseCategoricalCrossentropy(from_logits=True),
                  metrics=['accuracy'])
    
    return model




class StopTrainingCallback(Callback):
    needStop = False
    def __init__(self, monitor='val_loss', value=0.00001, verbose=0):
        super(Callback, self).__init__()
        self.monitor = monitor
        self.value = value
        self.verbose = verbose

    def on_epoch_end(self, epoch, logs={}):
        #lossValue = logs.get(self.monitor)
        #print("on_epoch_end called....")
        if self.needStop == True:
            self.model.stop_training = True

   
def Train(model, train_images, train_labels, epochEndCallback = None, maxEpochs = None, verbose=1):
    lastCallback = StopTrainingCallback()
    lastCallback.needStop = False
    
    callbacks = None
    if epochEndCallback is not None:
        callbacks = [lastCallback,epochEndCallback]
    else:
         callbacks = [lastCallback]
         
    if maxEpochs is None:
        maxEpochs = 50
    model.fit(train_images, train_labels, epochs=maxEpochs,verbose=verbose,callbacks=callbacks)
    return model

def StopTrain(model):
    model.stop_training = True
    return 1
    
def Test(model, test_images, test_labels):
    test_loss, test_acc = model.evaluate(test_images,  test_labels, verbose=0)
    return test_acc

def Predict_test(model, test_images, test_labels):
    #Predict
    #probability_model = tf.keras.Sequential([model, tf.keras.layers.Softmax()])
    predictions = model.predict(test_images)
    
    #Show predicted results
    nrCorrectPredictions = 0
    for i in range(1, len(predictions), 1):
        #print("Predicted / correct: ", np.argmax(predictions[i]), test_labels[i]);
        if np.argmax(predictions[i]) == test_labels[i]:
            nrCorrectPredictions += 1;
    #print("Correct guessed: ", nrCorrectPredictions, " / ", len(test_images))
            
            
def Predict(model, images):
    #Predict
    #probability_model = tf.keras.Sequential([model, tf.keras.layers.Softmax()])
    predictions = model.predict(images)  
    
    answer = []
    #Show predicted results
    for i in range(0, len(predictions), 1):
        index = np.argmax(predictions[i])
        char = None
        if index <=25:
            char = chr(ord('A') + index)
        else:
            char = chr(ord('0') + index - 26)
        #print("Guessed ", i, ": ", char, "; Index: ", index)
        answer.append(char)
    return answer


def Save(model, filePath):
    model.save(filePath,overwrite=True)


def Load(filePath):
    model = tf.keras.models.load_model(filePath,compile=False)
    #Settings some things before training
    model.compile(optimizer='adam',
                  loss=tf.keras.losses.SparseCategoricalCrossentropy(from_logits=True),
                  metrics=['accuracy'])
    return model