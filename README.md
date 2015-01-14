Tosan TFSHelper
=========

As one of the TFS zeal developer and user, I have always been eager to improve TFS api to bring more capability for this excellent ALM system. 
TFS's api although being well-built and highly programmable, with the experince that I have gained developing and customizing different software (specially Microsoft CRM), the lack of more maintainable plugin system in TFS's api was obvious me. So I have created this module to improve the ease of creating and managing plugins for TFS; moreover, the lack of Validation Plugins (Plugins that validate the event before being done) in TFS is the key decision point for my organization (whcih being included in this module).

this comperhensive package (or module) includes an event aggregator system that sit on top of Team Foundation Server (TFS) api.

This set of services facilitates process of creating a module (as a plugin) in the TFS.
The possible plugins that could be acheived through this module are: WorkItem plugins (Sync/Async), Build Plugins, and CheckIn/CheckOut Plugins.

One of the unique features of this module that haven't been incorporated in TFS intself is, the possibility of creating Validation Plugins (Synchronous Plugins) alongside with asychronous plugins. Such a plugin provides the ability to throw an error message before an event action being done and cancel the action eventualy.

This module have been tested with Team Foundation Server Version 2013.2-2013.3 but there are no reasons using it with older version of TFS (2012/2010) (Needs rebuild with the older TFS's assemblies).

This is an inprogress project and I'm working on it at the moment. 
I have a plan to write a thorough help and technical documents for application of this module in near future.

//Todo: Complete the help (readme).
