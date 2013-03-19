GitDeployHub for IIS
====================

Simple web app that can listen for web requests (like github webhooks) and trigger git deployments using a ```git pull```.

Requirements
------------

- IIS 6+
- git CLI on system PATH (eg. [git for windows](https://code.google.com/p/msysgit/downloads/list?q=full+installer+official+git))

Installation
-------------

1. git clone this repository
``` sh
cd D:\Inetpub\wwwroot
git clone git://github.com/benjamine/gitdeployhub.git
```
3. publish on IIS (eg. HomeDirectory= ```D:\Inetpub\wwwroot\gitdeployhub\Web\```)
4. setup this site using .Net 4.0 and add Write permissions on his own folder, and on any other folder you want to deploy websites to.
5. open the site homepage for instructions on adding deploy targets (instances).
