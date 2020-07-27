# -*- coding: utf-8 -*-
"""
    Created on Wed Jun 10 20:38:23 2020
    
    @author: Ghercioglo "Romeon0" Roman
"""


import wx
import ApplicationController as appController #INFO: Our app controller
import cv2 as cv
import numpy as np
from tensorflow.keras.callbacks import Callback
import logging

class Subscriber:
    def __init__(self, name):
        self.name = name
    def update(self, arg1=None, arg2=None, arg3=None):
        print('empty empty func')
        
class Publisher:
    def __init__(self):
        self.subscribers = set()
    def register(self, who):
        self.subscribers.add(who)
    def unregister(self, who):
        self.subscribers.discard(who)
    def dispatch(self, arg1=None, arg2=None, arg3=None):
        for subscriber in self.subscribers:
            subscriber.update(arg1, arg2, arg3)
            


class EpochEndCallback(Callback):
    
    publisher = Publisher()
    
    def Subscribe(self, func):
        self.publisher.register(func)
 
    
    def __init__(self, monitor='val_loss', value=0.00001, verbose=0):
        super(Callback, self).__init__()
        self.monitor = monitor
        self.value = value
        self.verbose = verbose

    def on_epoch_end(self, epoch, logs={}):        
        lossValue = logs['loss']    
        accValue = logs['accuracy']   
        self.publisher.dispatch(epoch, lossValue, accValue)
        
           


def GeBitmap(path, width=None, height=None):
    image = wx.Bitmap(path, wx.BITMAP_TYPE_ANY)
    if width is not None:
        image = Rescale(image, width, height)
    result = wx.Bitmap(image)
    return result
    
def CvImageToWxBitmap(image, isGray=False):
    height, width = image.shape[:2]
    bmp = image
    if isGray is True:
        bmp = cv.cvtColor(bmp, cv.COLOR_GRAY2RGB) 
    bmp = wx.Bitmap.FromBuffer(width, height, bmp)
    return bmp

def Rescale(image, width, height):
    image = image.ConvertToImage()
    image = image.Scale(width, height, wx.IMAGE_QUALITY_HIGH)
    image = wx.Bitmap(image)
    return image
def CreateEmptyBitmap(width, height, color = None):
    if color is None:
        return wx.Bitmap.FromRGBA(width, height, alpha=254)
    else:
        return wx.Bitmap.FromRGBA(width, height, alpha=254, red=color[0], green=color[1], blue=color[2])
    


class Application(wx.Frame,Subscriber):    
    #Network
    currNetworkModel = None
    isTraining = False
    lastMaxEpochs = None
    
    def __init__(self):
        super().__init__(parent=None,size=(500,500), title='Neural Network - Number Plate Detection')
        panel = wx.Panel(self)        
        
        #Close when x clicked
        self.Bind(wx.EVT_CLOSE, self.OnClose)
        
        path = "./Data/Cars/car (1).jpg"

        
        
        #self.text_ctrl = wx.TextCtrl(panel)
        #my_sizer.Add(self.text_ctrl, 0, wx.ALL | wx.EXPAND, 5)    
        
        # wx.SpinCtrl(self, id=5, value="3", pos=(100, 100),
        #  size=(100,100), style=wx.SP_ARROW_KEYS, min=0, max=100, initial=32,
        #  name="wxSpinCtrl")
     
        #Elements ---------------

        #Main
        mainSizer = wx.BoxSizer(wx.VERTICAL)  
        
                
        #Info
        self.txtEpoch = wx.StaticText(panel, label="Epoch: -")
        self.txtEpochError = wx.StaticText(panel, label="Epoch error: -") 
        self.txtEpochAccuracy = wx.StaticText(panel, label="Epoch accuracy: -") 
        self.txtMaxEpochs = wx.StaticText(panel, label="Max epochs:") 
        self.spinMaxEpochs = wx.SpinCtrl(panel, style=wx.SP_ARROW_KEYS, min=1, max=500, initial=10)
        networkInfoSizer = wx.BoxSizer(wx.HORIZONTAL)  
        networkSizer_1 = wx.BoxSizer(wx.VERTICAL)  
        networkSizer_2 = wx.BoxSizer(wx.VERTICAL)  
        networkSizer_1.Add(self.txtEpoch, 0, wx.ALL, 5)
        networkSizer_1.Add(self.txtEpochError, 0, wx.ALL, 5)
        networkSizer_1.Add(self.txtEpochAccuracy, 0, wx.ALL, 5)
        networkSizer_2.Add(self.txtMaxEpochs, 0, wx.ALL, 5)
        networkSizer_2.Add(self.spinMaxEpochs, 0, wx.ALL, 5)
        networkInfoSizer.Add(wx.StaticLine(panel, -1, style=wx.LI_VERTICAL,size=(2,100)), 0, wx.ALL, 5)
        networkInfoSizer.Add(networkSizer_1, 0, wx.ALL, 5)
        networkInfoSizer.Add(wx.StaticLine(panel, -1, style=wx.LI_VERTICAL,size=(2,100)), 0, wx.ALL, 5)
        networkInfoSizer.Add(networkSizer_2, 0, wx.ALL, 5)
        networkInfoSizer.Add(wx.StaticLine(panel, -1, style=wx.LI_VERTICAL,size=(2,100)), 0, wx.ALL, 5)
        mainSizer.Add(networkInfoSizer, 0, wx.ALL | wx.CENTER, 5)
        
        #--------
        #Input image
        self.txtInputImage = wx.StaticText(panel, label="Input Image")
        self.imgInput = wx.StaticBitmap(panel, -1, CreateEmptyBitmap(200,200, (255,255,255)))
        inputImageSizer = wx.BoxSizer(wx.VERTICAL)   
        inputImageSizer.Add(self.txtInputImage, 0, wx.ALL, 5)
        inputImageSizer.Add(self.imgInput, 0, wx.ALL, 5)  
               
        #Plate image
        self.txtPlate = wx.StaticText(panel, label="Plate Image") 
        self.imgPlate = wx.StaticBitmap(panel, -1, CreateEmptyBitmap(200,100, (255,255,255)))
        plateSizer = wx.BoxSizer(wx.VERTICAL)  
        plateSizer.Add(self.txtPlate, 0, wx.ALL, 5)
        plateSizer.Add(self.imgPlate, 0, wx.ALL, 5)  
        
          
        #Characters
        self.imgCharacters = []
        for i in range(1, 12, 1):
            imgPath = "./Data/Numbers/0/o_nt ("+str(i)+").png"
            img = wx.StaticBitmap(panel, -1, CreateEmptyBitmap(60,60, (255,255,255)))
            self.imgCharacters.append(img)
        self.txtCharacters = wx.StaticText(panel, label="Characters") 
        charactersSizer_1 = wx.BoxSizer(wx.HORIZONTAL)  
        charactersSizer_1.Add(self.txtCharacters, 0, wx.ALL, 5) 
        charactersSizer_2 = wx.GridSizer(4,5,5)
        for img in self.imgCharacters:
            charactersSizer_2.Add(img, 0, wx.ALL, 5)
        charactersSizerMain = wx.BoxSizer(wx.VERTICAL)  
        charactersSizerMain.Add(charactersSizer_1, 0, wx.ALL, 5)
        charactersSizerMain.Add(charactersSizer_2, 0, wx.ALL, 5)

        imagesSizer = wx.BoxSizer(wx.HORIZONTAL) 
        imagesSizer.Add(inputImageSizer, 0, wx.ALL, 5)  
        imagesSizer.Add(plateSizer, 0, wx.ALL, 5)  
        imagesSizer.Add(charactersSizerMain, 0, wx.ALL, 5) 
        mainSizer.Add(wx.StaticLine(panel), 0, wx.ALL|wx.EXPAND, 5)
        mainSizer.Add(imagesSizer, 0, wx.ALL | wx.CENTER, 5) 
        #--------
        
        #Recognized 
        self.txtRecognizedNumber = wx.StaticText(panel, label="Recognized: -") 
        mainSizer.Add(self.txtRecognizedNumber, 0, wx.ALL | wx.CENTER, 5) 
        
        #Navigation
        self.btnStartTrain = wx.Button(panel, label='Start Train')
        self.btnStopTrain = wx.Button(panel, label='Stop Train')
        self.btnRecognize = wx.Button(panel, label='Recognize')
        self.btnInitNetwork = wx.Button(panel, label='Init network')
        self.btnLoadNetwork = wx.Button(panel, label='Load network')
        self.btnLoadBestNetwork = wx.Button(panel, label='Load best network')
        self.btnSaveNetwork = wx.Button(panel, label='Save network')
        buttonsSizer = wx.BoxSizer(wx.HORIZONTAL) 
        buttonsSizer.Add(self.btnStartTrain, 0, wx.ALL, 5)    
        buttonsSizer.Add(self.btnStopTrain, 0, wx.ALL, 5) 
        buttonsSizer.Add(wx.StaticLine(panel, -1, style=wx.LI_VERTICAL,size=(2,20)), 0, wx.ALL, 5)
        buttonsSizer.Add(self.btnRecognize, 0, wx.ALL, 5)  
        buttonsSizer.Add(wx.StaticLine(panel, -1, style=wx.LI_VERTICAL,size=(2,20)), 0, wx.ALL, 5)
        buttonsSizer.Add(self.btnInitNetwork, 0, wx.ALL, 5)   
        buttonsSizer.Add(self.btnLoadNetwork, 0, wx.ALL, 5)   
        buttonsSizer.Add(self.btnLoadBestNetwork, 0, wx.ALL, 5)  
        buttonsSizer.Add(self.btnSaveNetwork, 0, wx.ALL, 5) 
        mainSizer.Add(wx.StaticLine(panel), 0, wx.ALL|wx.EXPAND, 5)
        mainSizer.Add(buttonsSizer, 0, wx.ALL | wx.CENTER, 5)  


        panel.SetSizer(mainSizer)  
        mainSizer.Fit(self)
        self.Show()
        
        self.SetListeners()
        
    
    def SetListeners(self):
        #btnStartTrain = None
        #btnStopTrain = None
        self.btnRecognize.Bind(wx.EVT_BUTTON, self.OnRecognizeClicked)
        self.btnStartTrain.Bind(wx.EVT_BUTTON, self.OnStartTrainClicked)
        self.btnStopTrain.Bind(wx.EVT_BUTTON, self.OnStopTrainClicked)
        self.btnInitNetwork.Bind(wx.EVT_BUTTON, self.OnInitNetworkClicked)
        self.btnLoadNetwork.Bind(wx.EVT_BUTTON, self.OnLoadNetworkClicked)
        self.btnLoadBestNetwork.Bind(wx.EVT_BUTTON, self.OnLoadBestNetworkClicked)
        self.btnSaveNetwork.Bind(wx.EVT_BUTTON, self.OnSaveNetworkClicked)
        
    def OnClose(self, event):
        self.Destroy()   
        self.StopAllProcesses()
        
        
    def update(self, epoch, err, acc):
        #print("Updated!")
        epoch = epoch + 1
        self.txtEpoch.SetLabel("Epoch: " + str(epoch))
        self.txtEpochError.SetLabel("Epoch error: {:.5f}".format(err))
        self.txtEpochAccuracy.SetLabel("Accuracy: {:.5f}".format(acc))
        if self.lastMaxEpochs == epoch:
            self.isTraining = False
        
        
    def StopAllProcesses(self):
         if self.currNetworkModel is not None and self.isTraining == True:
            appController.StopTrain(self.currNetworkModel)
            self.currNetworkModel = None
    
    def AlertNotInitializedNetwork(self):
        message = "You need to initialize model. Use 'Init Network', 'Load Network' or 'Load Best Network'"
        caption = "Model not initialized"
        dialog = wx.MessageDialog(self, message, caption=caption, style=wx.OK | wx.ICON_EXCLAMATION)
        result = dialog.ShowModal()
    def AlertNotTraining(self):
       message = "Network is not training now"
       caption = "No training"
       dialog = wx.MessageDialog(self, message, caption=caption, style=wx.OK | wx.ICON_INFORMATION)
       result = dialog.ShowModal()
       
    def AnswerStopProcesses(self):
       message = "All processes(training/testing/recognizing) now running will be stopped. Continue?"
       caption = "Processes will be stopped"
       dialog = wx.MessageDialog(self, message, caption=caption, style=wx.OK | wx.CANCEL | wx.ICON_INFORMATION)
       result = dialog.ShowModal()
       return result
    def PickFile(self, extensions = None):
        wildcard = "All files (*)"
        if extensions is not None:
            desc = "Files ("
            exts = "|"
            for index in range(0, len(extensions), 1):
                ext = extensions[index]
                desc += "*." + ext + ",";
                exts += "*." + ext + ";";
            desc = desc[:-1] + ')'
            exts = exts[:-1]
            wildcard = desc + exts #"Image files (*.png,*.jpg,*.jpeg)|*.jpg;*.png;*.jpeg"
        with wx.FileDialog(self, "Open file", wildcard=wildcard,
                               style=wx.FD_OPEN | wx.FD_FILE_MUST_EXIST) as fileDialog:
        
            if fileDialog.ShowModal() != wx.ID_CANCEL:
                path = fileDialog.GetPath()
                return path
        return None
    
    
    def SaveFile(self, extensions = None):
        wildcard = "All files (*)"
        if extensions is not None:
            desc = "Files ("
            exts = "|"
            for index in range(0, len(extensions), 1):
                ext = extensions[index]
                desc += "*." + ext + ",";
                exts += "*." + ext + ";";
            desc = desc[:-1] + ')'
            exts = exts[:-1]
            wildcard = desc + exts #"Image files (*.png,*.jpg,*.jpeg)|*.jpg;*.png;*.jpeg"
        with wx.FileDialog(self, "Save file", wildcard=wildcard,
                               style=wx.FD_SAVE | wx.FD_OVERWRITE_PROMPT) as fileDialog:
        
            if fileDialog.ShowModal() != wx.ID_CANCEL:
                path = fileDialog.GetPath()
                return path
                     
        return None
    
    

    def OnRecognizeClicked(self, event):
        if(self.currNetworkModel==None):
            self.AlertNotInitializedNetwork()
            return
        
        path = self.PickFile(("png", "jpg", "jpeg"))
        
        if path is None:
            return
        
        self.imgInput.SetBitmap(GeBitmap(path, 200, 200))
        
        print("Input params: ", path, self.currNetworkModel)
        plate, characters, result = appController.RecognizeNumber(path, self.currNetworkModel)
        if plate is not None:
            self.imgPlate.SetBitmap(CvImageToWxBitmap(plate, True))
            counter = 0
            
            #Set result message
            charsCountFound = -1
            if characters is not None: 
                charsCountFound = len(characters)-1
                self.txtRecognizedNumber.SetLabel("Recognized: " + result)
            else:
                charsCountFound = -1
                self.txtRecognizedNumber.SetLabel("Recognized: characters on plate not found.")
            
            #Show characters
            for index in range(0, len(self.imgCharacters), 1):
                if index <= charsCountFound:  
                    char = characters[index]
                    bmp = Rescale(CvImageToWxBitmap(char, True), 60, 60)
                    self.imgCharacters[counter].SetBitmap(bmp)
                else:
                     self.imgCharacters[counter].SetBitmap(CreateEmptyBitmap(60, 60))
                counter += 1
        else:
            self.txtRecognizedNumber.SetLabel("Recognized: plate not found!")
            for index in range(0, len(self.imgCharacters), 1):
                self.imgCharacters[index].SetBitmap(CreateEmptyBitmap(60, 60))
            self.imgPlate.SetBitmap(CreateEmptyBitmap(200, 100))

    def OnStartTrainClicked(self, event):
        if(self.currNetworkModel==None):
            self.AlertNotInitializedNetwork()
            return
        else:
            if self.isTraining == False:
                callback = EpochEndCallback()
                callback.Subscribe(self)
                
                self.isTraining = True
                maxEpochs = self.spinMaxEpochs.GetValue()
                self.lastMaxEpochs = maxEpochs
                self.currNetworkModel = appController.Train(callback, maxEpochs, self.currNetworkModel)
     
    def OnStopTrainClicked(self, event):
        if(self.currNetworkModel==None):
            self.AlertNotTraining()
            return
        
        if self.isTraining == True:
            appController.StopTrain(self.currNetworkModel)
            self.isTraining = False
       
    def OnInitNetworkClicked(self, event):
        if self.AnswerStopProcesses() == wx.ID_OK:
            self.StopAllProcesses()
            self.currNetworkModel = appController.CreateNewNetwork()
        
    def OnLoadNetworkClicked(self, event):
        if self.AnswerStopProcesses() == wx.ID_OK :
            self.StopAllProcesses()
            path = self.PickFile(["network"])
            if path is not None:
                self.currNetworkModel = appController.LoadNetwork(path)
     
    def OnLoadBestNetworkClicked(self, event):
        if self.AnswerStopProcesses() == wx.ID_OK:
            self.StopAllProcesses()
            path = "./Data/Networks/NNModel_Both_best.network"
            self.currNetworkModel = appController.LoadNetwork(path)
    def OnSaveNetworkClicked(self, event):
        if(self.currNetworkModel==None):
           self.AlertNotInitializedNetwork()
           return     
        path = self.SaveFile(["network"])
        if path is not None:
            appController.SaveNetwork(self.currNetworkModel, path)   
        
        
        
if __name__ == '__main__':
    app = wx.App(False)
    frame = Application()
    app.MainLoop()