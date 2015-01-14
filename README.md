Tosan TFSHelper
=========

As one of the TFS zeal developer and users I have always been eager to improve TFS api. With the experince that I have gained using different software the lack of robust and more maintainable 

A comperhensive package includes a event aggregator system that sit on top of Team Foundation Server (TFS) services.

This set of services facilitates process of creating a module (as a plugin) in the TFS.
The possible plugins that could be acheived through this package are: WorkItem plugins (Sync/Async), Build Plugins and CheckIn/CheckOut Plugins.

One of the unique features of this module that haven't been incorporated in TFS intself is, the possibility of creating Validation Plugins (Synchronous Plugins) besides normal asychronous plugin. Such a plugin provides the ability to show an error message before an event action being done and cancel the action eventualy.

This module have been tested with Team Foundation Server Version 2013.2-2013.3 but shouldn't be any problem using it with older version of TFS (2012/2010) also (Needs rebuild with the older TFS's assemblies).

This is an inprogress project and I'm working on it at the moment. 
I have a plan to write a thorough help and technical documents for application of this module in near future.

//Todo: Complete the help (readme).
