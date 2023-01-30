# ssh-tester
Test SSH Connection with Ping Response and Port Checking and Exchange HostKey and Getting Fingerprint
I use SshClient Class From SSH.NET https://github.com/omidmohajers/SSH.NET create by @sshnet : https://github.com/sshnet

## how to use
for start create instance of 'SshConnectionChecker' and assign 'SshProfile' instance to properties
then listen to : 'LogChanged' and 'Passed' events to getting connection information 
for start operation call 'ConnectAsync' method

## Example
connectionChecker = new SshConnectionChecker();
                connectionChecker.AnyPort = RunAsAnyPortMethod;
                connectionChecker.Passed += ConnectionChecker_Passed;
                connectionChecker.LogChanged += ConnectionChecker_LogChanged;
                connectionChecker.Profile = prof;
                connectionChecker.ConnectAsync();
                
note : SshProfile maybe has a too many ports and SshConnectionChecker check all ports and responsibility if you want finish process when any port response make 'AnyPort' = 'true'

## performance
After calling the ConnectAsync method, SshConnectionChecker first pings the address of the SshProfile server, and if it receives a suitable response, it will then receive the HostKey from the specified server and ports async.

## apllication

### v10.0.0
Link : https://github.com/omidmohajers/ssh-tester/releases/tag/v1.0.0
PA.SSH.Wpf.exe using this tool to check many ssh server with many ports for use start the app
![image](https://user-images.githubusercontent.com/120931404/215487925-19c43cce-0273-42fd-b2ef-aa3cfe655f56.png)
application load default servers list from input.txt, if you need to load deffrent list click on 'Load' button and select file
file lines must be in this format : [profile name],[server address],[username],[password],[list of ports seperated by ':' ]. if you dont have a username and password leave them empty
you can change data by doubleclick on them after change you need to save changes for save you have two options:
###### Save : save changes to 'input.txt'
###### SaveAs : save changes to selected path

before start operation check or uncheck 'Any Port Response Method' 
after start app try to get ping from address if server reply 75% sucessfully app try connect to server and get hostkey and fingerprint data from server
all successfull result shown on right panel and other message shown on left botton panel
you can save result by best ping order or best response order to text file

![image](https://user-images.githubusercontent.com/120931404/215493628-8cdc21df-95ae-4986-8e4c-b4b7bdb9e4bb.png)
