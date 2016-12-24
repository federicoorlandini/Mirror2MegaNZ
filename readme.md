# Mirror2MegaNZ

Tool to backup your files on [MegaNz](https://mega.nz/) accounts.
MegaNZ gives to you free accounts of 50Gbyte each. Using this tool you can back up all your file in the Cloud even if you have more than 50 Gbyte of data to back up.
This tool uses the [MegaApiClient](https://github.com/gpailler/MegaApiClient) from [Grégoire Pailler](https://github.com/gpailler).

## How it works
These are the steps to back up your files using different MegaNZ account.
- Create local folders and move your file in each folder so that the overall size of each folder is less or equal 50 Gbyte. For eaxmple:
-- Folder1
-- Folder2
- Configure the Mirror2MegaNZ to map each folder on a different account. For example:
-- Folder1 --> Account 1
-- Folder2 --> Account 2
- Execute the tool
Check the next paragraph to know how to configure Mirror2MegaNZ to map the folders on the MegaNZ accounts.
Every local folder structure will be replicated in the remote MegaNZ accounts.

## Configuration
The configuration is in the _App_Data\account.json_ file. The file has the following structure:
```sh
[
  {
    "LocalRoot": "D:\path\to\folder1",
    "Name": "Account #1 name",
    "Password": "xxxxxxxxxxx",
    "Username": "xxxxx@yyyyy.com",
    "Synchronize": "true"
  },
  {
    "LocalRoot": "D:\path\to\folder2",
    "Name": "Account #2 name",
    "Password": "xxxxxxxxxxx",
    "Username": "yyyyyy@yyyyy.com",
    "Synchronize": "true"
  },
  {
    "LocalRoot": "D:\path\to\folder3",
    "Name": "Account #3 name",
    "Password": "xxxxxxxxxxx",
    "Username": "zzzzzz@yyyyy.com",
    "Synchronize": "true"
  }
]
```
Here there is an explaination about all the configuration parameter:
- _LocalRoot_: the path to the local folder that you wanto to mirror in the MegaNZ account
- _Name_: a name of the account (for example: Photo Summer 2015, Documents, etc.)
- _Username_ and _Password_: the credentials to access the specific MegaNZ account
- _Synchronize_: if true, the tool will synchronize the files in the remote folder with the files in the local folder. If false, the specific folder will be skipped.

## Notes
- The local folder will never be affected: no files will be created or deleted in the local folder;
- Before starting to synchronize the files, the tool will show you a resume of the actions it will take. You can decide to continue or not.

## TODO
- Add a retry policy when uploading the files;
- Add logging on local text files;
- Add an initial check to control if the MegaNZ account has enough free space to complete the synchronization;
