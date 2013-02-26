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
2. compile
3. publish on IIS (on the machine were you want to do deployments)
4. this site will need privileges to Write on his own folder, and run ```npm pull``` on your target folders.
5. open the site homepage for instructions on adding deploy targets (instances).

