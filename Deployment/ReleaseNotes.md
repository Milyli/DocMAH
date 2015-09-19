DocMAH Release Notes
====================

*Unless otherwise noted, the release notes for each release contain links to the [GitHub issues][1] addressed.*

## 0.9.9.0 Popup Button Configuration
1. I24 - [Hide Got It or Remind Me Later buttons.][27]
1. I23 - [Set labels for Got It / Remind Me Later buttons][28]

## 0.9.8.0 Maintenance Release
1. B59 - [NuGet package does not install if modules element not present][25]
1. B60 - [jQuery interferes with existing jQuery libraries.][26]

## 0.9.7.0 Maintenance Release
1. B55 - [Stop Replacing Connection String Name on Package Upgrade][23]
		 **Note -** Due to NuGet behavior, this will first take effect on the next release.
		 However, it was fixed here, so here the note goes.
1. I56 - [Minor UI Style Updates][24]

## 0.9.6.0 Look and Feel Update
1. I30 - [Improve Design and Usability of Documentation Page][16]
1. B51 - [Error on content install when reusing URLs][17]
1. B52 - [Popups and bullets are placed off document][18]
1. I47 - [Make all tests use LocalDB][19]
1. I49 - [Turn off Documentation Page][20]
1. I54 - [Remove JavaScript minification mapping.][21]
1. I46 - [Cache Documentation Page and First Time Help Content][22]

## 0.9.5.0 Maintenance Release
1. B36 - [Page URLs deleted on page reorder.][10]
1. B40 - [Install script generation does not require edit permission.][11]
1. I38 - [Improve Testability of Request Processors][12]
		 **Breaking Change - Low Impact -** One of the many changes for testability required
		 modification of the content deployment file format. The new version of the application 
		 is *not* backwards compatible. However, as long as you regenerate your content 
		 deployment file and deploy it with this version of DocMAH, there will be no issues.
		 **Breaking Change - Low Impact -** The content deployment file was renamed 
		 from ApplicationHelpInstall.xml to DocmahContent.xml. Similar to above, make sure to 
		 regenerate your content file and deploy it with your new DocMAH DLL.
1. I45 - [Open Documentation in New Window/Tab][13]
1. B43 - [Not Working in ASP.NET Classic][14]
1. I37 - [Remove first time help from documentation pages][15]

## 0.9.4.0 Maintenance Release
1. I5 - [Return to application from documentation.][6]
1. B33 - [Fixed error in fail-over draggable anchor jQuery selector.][7]
1. B34 - [Downloaded install script name fixed.][8]

## 0.9.3.0 Maintenance Release
1. I31 - [Added NuGet icon.][4]
1. B32 - [Modal mask now covers document.][5]

## 0.9.2.0 Maintenance Release
1. B26 - [ToC Icons no longer repeat in Chrome.][2]
1. B27 - [Error message when no anchors available.][3]

## 0.9.1.0 First NuGet Release
1. [NuGet pacakge created.][9]

High level feature sets include:

+ First time help popups and editing.
+ Documentation pages and editing with HTML content.
+ Database and content versioning and installation.

[1]: https://github.com/Milyli/DocMAH/issues
[2]: https://github.com/Milyli/DocMAH/issues/26
[3]: https://github.com/Milyli/DocMAH/issues/27
[4]: https://github.com/Milyli/DocMAH/issues/31
[5]: https://github.com/Milyli/DocMAH/issues/32
[6]: https://github.com/Milyli/DocMAH/issues/5
[7]: https://github.com/Milyli/DocMAH/issues/33
[8]: https://github.com/Milyli/DocMAH/issues/34
[9]: https://github.com/Milyli/DocMAH/issues/20
[10]: https://github.com/Milyli/DocMAH/issues/36
[11]: https://github.com/Milyli/DocMAH/issues/40
[12]: https://github.com/Milyli/DocMAH/issues/38
[13]: https://github.com/Milyli/DocMAH/issues/45
[14]: https://github.com/Milyli/DocMAH/issues/43
[15]: https://github.com/Milyli/DocMAH/issues/37
[16]: https://github.com/Milyli/DocMAH/issues/30
[17]: https://github.com/Milyli/DocMAH/issues/51
[18]: https://github.com/Milyli/DocMAH/issues/52
[19]: https://github.com/Milyli/DocMAH/issues/47
[20]: https://github.com/Milyli/DocMAH/issues/49
[21]: https://github.com/Milyli/DocMAH/issues/54
[22]: https://github.com/Milyli/DocMAH/issues/46
[23]: https://github.com/Milyli/DocMAH/issues/55
[24]: https://github.com/Milyli/DocMAH/issues/56
[25]: https://github.com/Milyli/DocMAH/issues/59
[26]: https://github.com/Milyli/DocMAH/issues/60
[27]: https://github.com/Milyli/DocMAH/issues/24
[28]: https://github.com/Milyli/DocMAH/issues/23