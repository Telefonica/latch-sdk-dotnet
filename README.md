# LATCH SDK INSTALLATION GUIDE FOR .NET

## INSTALLING THE MODULE LATCH

### PREREQUISITES 
- Microsoft .Net Framework 4 or later.

- To get the **"Application ID"** and **"Secret"**, (fundamental values for integrating Latch in any application), it’s necessary to register a developer account in [Latch's website](https://latch.tu.com). On the upper right side, click on **"Developer area"**. 


### DOWNLOADING THE SDK
* When the account is activated, the user will be able to create applications with Latch and access to developer documentation, including existing SDKs and plugins. The user has to access again to [Developer area](https://latch.tu.com/www/developerArea), and browse his applications from **"My applications"** section in the side menu.

* When creating an application, two fundamental fields are shown: **"Application ID"** and **"Secret"**, keep these for later use. There are some additional parameters to be chosen, as the application icon (that will be shown in Latch) and whether the application will support OTP (One Time Password) or not.

* From the side menu in developers area, the user can access the **"Documentation & SDKs"** section. Inside it, there is a **"SDKs and Plugins"** menu. Links to different SDKs in different programming languages and plugins developed so far, are shown.


### USING THE SDK IN C#.
* Create a Latch object to use sync operations, or LatchAsync object to async operations, with the "Application ID" and "Secret" previously obtained.
```
     Latch latch = new Latch(APP_ID, SECRET);
```
```
     LatchAsync latch = new LatchAsync(APP_ID, SECRET);
```

* Call to Latch Server with sync or async methods. Pairing will return an **Account ID** that you should store for future API calls
```
     LatchResponse pairResponse = latch.Pair(TOKEN);
     LatchResponse statusResponse = latch.Status(ACCOUNT_ID);
     LatchResponse opStatusResponse = latch.OperationStatus(ACCOUNT_ID, OPERATION_ID);
     LatchResponse unpairResponse = latch.Unpair(ACCOUNT_ID);
```
```
     LatchResponse pairResponse = await latch.PairAsync(TOKEN);
     LatchResponse statusResponse = await latch.StatusAsync(ACCOUNT_ID);
     LatchResponse opStatusResponse = await latch.OperationStatusAsync(ACCOUNT_ID, OPERATION_ID);
     LatchResponse unpairResponse = await latch.UnpairAsync(ACCOUNT_ID);
```


## RESOURCES
- You can access Latch's use and installation manuals, together with a list of all available plugins here: [https://latch.tu.com/www/developers/resources](https://latch.tu.com/www/developers/resources)

- Further information on de Latch's API can be found here: [https://latch.tu.com/www/developers/doc_api](https://latch.tu.com/www/developers/doc_api)
