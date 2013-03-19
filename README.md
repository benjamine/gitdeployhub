GitDeployHub for IIS
====================

Simple web app that can listen for web requests (like github webhooks) and trigger git deployments using a ```git fetch``` and ```git checkout```.
You can deploy from any git "treeish" (branch, tag or commit hash).

Requirements
------------

- IIS 6+
- git CLI on system PATH (eg. [git for windows](https://code.google.com/p/msysgit/downloads/list?q=full+installer+official+git))

Installation
-------------

1. git clone this repository:
``` powershell
cd D:\Inetpub\wwwroot
git clone git://github.com/benjamine/gitdeployhub.git
```

2. publish on IIS (eg. HomeDirectory= ```D:\Inetpub\wwwroot\gitdeployhub\Web\```)
 - set the site on his own .Net 4.0 Application Pool
 - add write permissions on his own folder, and on any other folder you want to deploy websites to. (the app will be running git fetch and checkout on those)
3. open the site homepage for instructions on adding deploy targets (instances).
