*** Doc(u)MAH-tation ***



** Prerequisites

1)
ASP.NET 4.0 (or above) web application - classic or MVC

2) 
SqlServer compatible database (SQL Server, SQL Express ...) 

We're working on other options. 
Let us know your preference at https://github.com/milyli/docmah



** Implementation 

1) 
Create config section, moodule, handler and docmah elements in web.config.
[Automatic] 
Barring bugs, this is handled by the NuGet package.
Just in case, here's the relevant configuration that's added.

<configuration>
	<configSections>
		<section name="docmah" type="DocMAH.Configuration.DocmahConfigurationSection, DocMAH"/>  
	</configSections>
	<system.web>
		<httpModules>
			<add name="DocMAH" type="DocMAH.Web.HttpModule, DocMAH"/>
		</httpModules>
		<httpHandlers>
			<add path="DocMAH.axd" verb="GET,POST" type="DocMAH.Web.HttpHandler, DocMAH"/>
		</httpHandlers>
	</system.web>
	<system.webServer>
		<modules>
				<add name="DocMAH" type="DocMAH.Web.HttpModule, DocMAH" preCondition="integratedMode" />
		</modules>
		<handlers>
				<add name="DocMAH" path="DocMAH.axd" verb="GET,POST" type="DocMAH.Web.HttpHandler, DocMAH" preCondition="integratedMode"/>
		</handlers>
	<system.webServer>
	<docmah connectionStringName="DefaultConnection" jsUrl="" cssUrl="">
		<documentation pageTitle="Documentation Pages" customCss=""/>
		<editHelp disabled="false" requireAuthentication="true" requireLocalConnection="true"/>
	</docmah>
</configuration>

2) 
Set a connection string. 
[Required] 
This is not handled by NuGet. 
You need only implement one of two options.

a) {preferred} web.config
	Set the connectionStringName attribute value of the docmah configuration element.
	Look for this bit in web.config --
	<docmah connectionStringName="DefaultConnection"

			- or -

b) {programmatic} DocMAH.Configuration.Configurator.ConnectionString = [connection string]
	preferably set in Global.asax Application_Start
	used when a connection string is only available programmatically. (super enterprisey-API-accessability type thing (hey, I needed this at work already))

3) 
Add a help button
[Optional, but recommended]

Create a help button (input, div, image, anchor, any element that supports jQuery click) for your pages or layout with the class dmhHelpButton
e.g.				<a class="dmhHelpButton" href="#">HELP ME! I'M LOST!</a>

Clicking the button shows either:
	a) The first time help for the current page if first time help has been created.
	b) Opens the documentation pages if first time help has not been created.

4)
Local resource bundles.
[Optional]

By default, the library injects links to the CDN hosted content listed below. 
This makes initial setup easier.
You may optionally link to the content as css and js bundles hosted on your own server.
This makes web sites more efficient.

JavaScript
	a) jQuery
	b) jQueryUi
	c) jsTree (for documentation page only)
CSS
	a) jsTree (for documentation page only)

5)
Create database schema.
[Automatic] (See Deployment section below for more details)

6)
You're done.



** Full DocMAH cofinguration description

The following are some notes about all of the configuration values.
	
<!-- docmah element - Top DocMAH custom configuration element. -->
<!-- connectionStringName attribute [Optional - sort of] Name of the connection string for database connections. If not set, must be set programmatically via DocMAH.Configuration.Configurator.ConnectionString on app startup.-->
<!-- jsUrl attribute [Optional - Default: renders individual links to MS CDN and cdnjs javascript libraries] Can override to a single URL (bundle) that contains jQuery, jQueryUi and jsTree script files. -->
<!-- cssUrl attribute [Optional - Default: renders individual link to cdnjs CDN css library] Can override to a single URL (bundle) that contains jsTree css default files. -->
<docmah connectionStringName="DefaultConnection" jsUrl="" cssUrl="">
	  
	<!-- documentation element - [Optional] Settings particular to the documentation page. -->
	<!-- pageTitle attribute [Optional - Default: Documentation] Provide a custom title for documentation page. -->
	<documentation pageTitle="Documentation Pages" customCss=""/>
	  
	<!-- editHelp element - [Optional] Controls the availability of edit mode for first time help and documentation. -->
	<!-- disabled attribute [Optional - Default: false] Disable all help editing for the application. Recommended for production environments. Overrides requireAuthentication and requireLocalConnection.-->
	<!-- requireAuthentication attribute [Optional - Default: true] Require authentication to edit help content. -->
	<!-- requireLocalConnection attribute [Optional - Default: true] Require a local connection to the server to edit help content. -->
	<editHelp disabled="false" requireAuthentication="true" requireLocalConnection="true"/>
  
</docmah>



** Help and Documenttaion Creation Pointers

1) 
By default, the "Create Help" button and documentation edit tools are shown if you are accessing your application from the machine hosting your application. 
Please read the notes on the editHelp configuration element in section 3 above to change this.

2)
Match URLs

Most of the settings for first time help entries are self explanatory. 
Match URLs, less so.
A Match URL links a particular first time help entry with the page(s) it should be shown on.
DocMAH makes a poor, naive guess as to the what the Match URLs value for each page should be.
For now, you will need to tweek the match URL for almost all applications.
Sorry, improving this is low on the priorities.

Match URLs support multiple space separated urls with optional wildcard (*).
For instance, a Match URLs value that works for all possible paths to the default ASP.NET MVC Home.Index action would be:
		/ /Home /Home/ /Home/Index*

URLs are first identified by exact matches.
If no match is found, a wildcard search is then executed.

All URLs must be unique in the database across all pages.

3) 
First time help frame and bullet locations are anchored relative to certain elements with Ids automatically. 
They will float relative to their anchor elements in a fluid layout.
The specific elements selected as anchors are elements with ids that do not have child elements.
Keeping the element set small improves the performance of the edit tools.
Please let us know at https://github.com/milyli.docmah if you can think of a better element set.

4) 
Documentation page table of contents items may be rearranged by clicking on titles (not icons) and dragging and dropping.

5) 
Right click table of contents items for more edit options.

6) 
First time help content may be included in the documentation pages without copying the information.
If included, a screen shot is needed on the server to show behind the help frame and bullets to provide context.
Web based image management is slated as a future feature.
The help frame and bullets must be repositioned relative to the image to mimic the live first time help.
The first time help entries can optionally be hidden in the documentation pages.



** Deployment **
On the documentation page, there is a button labeled "Generate Install Scripts".
Clicking this button does two things.
	a) DocmahContent.xml is created in your application's root folder.
		If you are writing help for the site you are developing, just make sure this file gets deployed and throw away option b.
	b) You are offered the option to save the file elsewhere on your computer through the browser.
		This was added so that documentation writers could work on a non-development environment, generate the file and deliver it to a developer to add to the build process.

The first time DocMAH is accessed, it builds and updates its own database tables based on a database version number compiled into the dll.

Immediately afterwards, DocMAH updates the help content based on the content and help content version number in the content deployment file.

It is recommended that you disable the edit tools entirely in production environments.
Look for this in web.config ->       <editHelp disabled="false"
See the full configuration description above for more information.



** Attributions **

1)
<div>Documentation icons made by <a href="http://www.freepik.com" title="Freepik">Freepik</a> from <a href="http://www.flaticon.com" title="Flaticon">www.flaticon.com</a> is licensed under <a href="http://creativecommons.org/licenses/by/3.0/" title="Creative Commons BY 3.0">CC BY 3.0</a></div>



** Miscellaneous Stuff **

1) 
This framework uses cookies to remember help content preferences for anonymous users. No cookie data is inspected or stored on the server by DocMAH.

2) 
Better Doc(u)MAH-tation on the way at https://docmah.net ... eventually.

3)
This is an MVP (http://en.wikipedia.org/wiki/Minimum_viable_product) equivalent release.
We released it on NuGet because we wanted to start using it in our professional and hobby sites.
We know there is a lot to be done.
If a feature is missing or you find a bug, up vote an eixting issue or create a new one at https://github.com/milyli/docmah