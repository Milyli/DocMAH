/** Namespace **/
var SH = SH || {};

/** Constants and Singletons **/
SH.PageTypes = function () {

	//** Public Fields **
	this.PopupPage = 1;
	this.DocumentationPage = 2;

	// Return singleton.
	return this;
}();

SH.HistoryState = function () {
	this.Replace = 1;
	this.Push = 2;
	this.Pop = 3;

	return this;
}();

SH.Urls = function () {

	// ** Public Fields **
	this.DeletePage = '/help.axd?m=DeletePage';
	this.GenerateInstallScript = '/help.axd?m=GenerateInstallScript';
	this.MovePage = '/help.axd?m=MovePage';
	this.NavigateDocumentation = '/help.axd?m=DocumentationPage';
	this.ReadApplicationSettings = '/help.axd?m=ReadApplicationSettings';
	this.ReadPage = '/help.axd?m=ReadPage';
	this.ReadTableOfContents = '/help.axd?m=ReadTableOfContents';
	this.SavePage = '/help.axd?m=SavePage';
	this.SaveUserPageSettings = '/help.axd?m=SaveUserPageSettings';

	// Return singleton.
	return this;
}();

SH.Helpers = function () {
	this.GetQueryParameter = function (name) {
		name = name.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
		var regex = new RegExp("[\\?&]" + name + "=([^&#]*)"),
			results = regex.exec(location.search);
		return results === null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
	}

	return this;
}();

/** Classes **/
SH.AjaxClient = function () {

	// ** Public Methods **
	this.DeletePage = function (pageId, successCallback) {
		$.ajax({
			type: 'POST',
			url: SH.Urls.DeletePage,
			data: pageId.toString(),
			success: successCallback,
		});
	}

	this.MovePage = function (pageId, newParentId, newPosition) {
		$.ajax({
			type: 'POST',
			url: SH.Urls.MovePage,
			data: JSON.stringify({
				PageId: pageId,
				NewParentId: newParentId,
				NewPosition: newPosition,
			}),
			async: false,	// synchronous so that moves complete before delete occurs when deleting nodes.
		});
	}

	this.ReadApplicationSettings = function (successCallback) {
		$.ajax({
			type: 'GET',
			url: SH.Urls.ReadApplicationSettings,
			success: successCallback,
			async: false,
		});
	}

	this.ReadPage = function (id, successCallback) {
		$.ajax({
			type: 'GET',
			data: { id: id },
			url: SH.Urls.ReadPage,
			success: successCallback,
		});
	}

	this.ReadTableOfContents = function (successCallback) {
		$.ajax({
			type: 'GET',
			url: SH.Urls.ReadTableOfContents,
			success: successCallback,
		});
	}

	this.SavePage = function (page, successCallback) {
		$.ajax({
			type: 'POST',
			contentType: "application/json",
			dataType: 'json',
			url: SH.Urls.SavePage,
			data: JSON.stringify(page),
			success: successCallback,
		});
	}

	this.SaveUserPageSettings = function (userPageSettings) {
		$.ajax({
			type: 'POST',
			contentType: "application/json",
			dataType: 'json',
			url: SH.Urls.SaveUserPageSettings,
			data: JSON.stringify(userPageSettings),
		});
	}
}

SH.AnchoredElementLogic = function () {
	var _elements = new SH.SharedElements();
	var self = this;

	// ** Private Methods ** 
	this.UpdateElementPosition = function (model, $element) {
		if ($element.length > 0) {
			var anchorOffset = $('#' + model.OffsetElementId).offset();
			var top = anchorOffset ? anchorOffset.top + model.VerticalOffset : 0;
			var left = anchorOffset ? anchorOffset.left + model.HorizontalOffset : 0;
			if (left < 0)
				left = 0;
			else if (left + $element.width() > $(document).width())
				left = $(document).width() - $element.width();

			$element.css({ top: top, left: left, });
		}
	}


	// ** Public Methods **
	this.UpdateElementPositions = function (pageModel, $form) {
		if (_elements.$getPopupMask().is(':visible')) {
			$.each(pageModel.Bullets, function (index, bullet) {
				self.UpdateElementPosition(bullet, $('#shDraggableBullet-' + bullet.Number));
				self.UpdateElementPosition(bullet, $('#shPlacedBullet-' + bullet.Number));
			});
			if ($form.is(':visible')) {
				self.UpdateElementPosition(pageModel, $form);
			}
		}
	}
}

SH.Cookies = function () {

	// ** Public Methods **
	this.DeleteCookie = function (cookieName) {
		document.cookie = cookieName + "=; expires=Thu, 01 Jan 1970 00:00:00 UTC";
	}

	this.GetCookie = function (cookieName) {
		var name = cookieName + "=";
		var ca = document.cookie.split(';');
		for (var i = 0; i < ca.length; i++) {
			var c = ca[i];
			while (c.charAt(0) === ' ') c = c.substring(1);
			if (c.indexOf(name) === 0) return c.substring(name.length, c.length);
		}
		return "";
	}

	this.SetCookie = function (cookieName, cookieValue, expirationDays) {
		var d = new Date();
		var cookie = cookieName + "=" + cookieValue;
		if (expirationDays) {
			d.setTime(d.getTime() + (expirationDays * 24 * 60 * 60 * 1000));
			var expires = "expires=" + d.toUTCString();
			cookie = cookie + "; " + expires;
		}
		document.cookie = cookie;
	}
}

SH.DocumentationPage = function () {

	// ** Private Fields **
	var _applicationSettings = null;
	var _currentPage = null;
	var _elements = new SH.SharedElements();
	var _newPage = null;		// Temporary new page when tree creates new page and name needs to be saved.
	var _originalPage = null;	// State of page before editing begins.
	var _popupView = null;		// Popup view to show popup help and modify documentation position.
	var _historyId = 1;			// Compared to popstate data to determine if forward or back was pressed.


	/** HTML Elements **/
	function $getCancelButton() { return $('#shCancelButton'); }
	function $getContentPanel() { return $('#shDocContentPanel'); }
	function $getDocumentationEditor() { return $('#shDocumentationEditor'); }
	function $getDocumentationEditorContent() { return $('#shDocumentationEditorContent'); }
	function $getDocumentationEditorTitle() { return $('#shDocumentationEditorTitle'); }
	function $getDocumentationTools() { return $('#shDocumentationTools'); }
	function $getDocumentationView() { return $('#shDocumentationView'); }
	function $getDocumentationViewContent() { return $('#shDocumentationViewContent'); }
	function $getDocumentationViewTitle() { return $('#shDocumentationViewTitle'); }
	function $getEditButton() { return $('#shEditButton'); }
	function $getEditModeButtonBar() { return $('#shEditModeButtonBar'); }
	function $getGenerateInstallScriptButton() { return $('#shGenerateInstallScriptButton'); }
	function $getIsHidden() { return $('#shIsHidden'); }
	function $getNewPageButton() { return $('#shNewPageButton'); }
	function $getPage() { return $('#shPage'); }
	function $getPageContainer() { return $('#shPageContainer'); }
	function $getPopupImage() { return $('#shPopupImage'); }
	function $getPopupImageUrl() { return $('#shPopupImageUrl'); }
	function $getPopupTools() { return $('#shPopupTools'); }
	function $getSaveButton() { return $('#shSaveButton'); }
	function $getTableOfContents() { return $('#shToc'); }
	function $getTitle() { return $('#shDocTitle'); }
	function $getToolBar() { return $('#shDocToolBar'); }
	function $getViewModeButtonBar() { return $('#shViewModeButtonBar'); }


	// ** Private Methods **

	// Reads the page model for the given node, selects the node in the toc and shows the page model.
	function ActivateNode(node, historyState) {
		historyState = historyState || SH.HistoryState.Push;

		var id = GetPageIdFromElementId(node.id);

		// if the clicked toc node id is valid (this is not a new page) and no page is loaded 
		//	or the node clicked does not represent the current page, request the page information.
		if (id && (!(_currentPage) || (_currentPage.Id !== id))) {
			var client = new SH.AjaxClient();
			client.ReadPage(id, function (page) {

				// Handle the browser history as needed.
				var url = "help.axd?m=DocumentationPage&id=" + page.Id;
				if (historyState === SH.HistoryState.Replace) {
					page.HistoryId = _historyId;
					history.replaceState(page, page.Title, url);
				} else if (historyState === SH.HistoryState.Push) {
					page.HistoryId = ++_historyId;
					history.pushState(page, page.Title, url);
				} else if (historyState === SH.HistoryState.Pop) {
					page.HistoryId = _historyId;	// History id already adjusted by Window_PopState handler.
				}

				// Reselect the current node. This only happens automatically is on initial tree load and mouse clicks on nodes.
				// It does not occur on browser back or id query parameter override.
				SelectNode(node);

				ShowPage(page);
			});
		}
	}

	function GetPageIdFromElementId(elementId) {
		return parseInt(elementId.substring("shTocEntry-".length));
	}

	// Sets edit tool bar to initial values and view mode for current page.
	function InitializeEditTools() {
		if (_applicationSettings &&
			_applicationSettings.CanEdit) {

			// Make space for the edit tool bar.
			var left = $getToolBar().outerWidth(true);
			$getContentPanel().css({ left: left + 'px', });
			$getTitle().css({ left: left + 'px', });

			// Show the edit tool bar.
			$getToolBar().show();

			// Reset all bars.
			$getPopupTools().hide();
			$getDocumentationTools().hide();
			$getEditModeButtonBar().hide();
			$getEditButton().hide();
			$getViewModeButtonBar().show();

			// If there is a current page, show the editor for the current page type.
			if (_currentPage) {
				$getEditButton().show();
				$getIsHidden()
					.prop('disabled', true)
					.prop('checked', _currentPage.IsHidden);

				if (_currentPage.PageType === SH.PageTypes.PopupPage) {
					$getPopupImageUrl()
						.prop('disabled', true)
						.val(_currentPage.DocImageUrl || '');

					$getPopupTools().show();
				} else if (_currentPage.PageType === SH.PageTypes.DocumentationPage) {
					$getDocumentationTools().show();
				}
			}
		}
	}

	function ResizeDocumentPage() {
		if (_currentPage) {
			var $container = $getPageContainer();
			var $page = $getPage();
			var height, width;

			if (_currentPage.PageType === SH.PageTypes.PopupPage) {
				var $background = $getPopupImage();
				// clientDimensions get value without scrollbar if present. 
				// Scrollbar padding is automatically added with height() and width() which is bad.
				height = Math.max($container.get(0).clientHeight, $background.height());
				width = Math.max($container.get(0).clientWidth, $background.width());
				$page.width(width).height(height);
				_elements.$getPopupMask().width(width).height(height);
			} else if (_currentPage.PageType === SH.PageTypes.DocumentationPage) {
				height = $container.get(0).clientHeight;
				width = $container.get(0).clientWidth;
				$page.width(width).height(height);
			}
		}
	}

	// Attempt to persist page model to server.
	// Reset the document to view mode for current page if successful.
	function SavePage() {
		if (_currentPage) {
			var client = new SH.AjaxClient();
			client.SavePage(_currentPage, function () {

				var node = $getTableOfContents().jstree('get_node', 'shTocEntry-' + _currentPage.Id);

				// Update table of contents UI as needed for each type.
				if (_currentPage.PageType === SH.PageTypes.PopupPage) {
					// Do nothing as of yet.
				} else if (_currentPage.PageType === SH.PageTypes.DocumentationPage) {
					$getTableOfContents().jstree('rename_node', node, _currentPage.Title);
				}

				// Set UI visibility indicator on current node. 
				// CSS handles changing style on children.
				if (_currentPage.IsHidden) {
					node.li_attr['class'] = 'tocHidden';
					$(('#' + node.id)).addClass('tocHidden');
				} else {
					node.li_attr['class'] = '';
					$(('#' + node.id)).removeClass('tocHidden');
				}

				ShowPage(_currentPage);

				$('#shStatusMessage')
					.html('Page saved.')
					.css('background-color', '#2d9639')
					.show()
					.delay(1000)
					.fadeOut('slow');
			});
		}
	}

	function SelectNode(node) {
		var tree = $getTableOfContents().jstree();
		var clearNodes = tree.get_selected(true);
		tree.deselect_node(clearNodes);
		tree.select_node(node);
	}

	// Sets up the content area for the current page type.
	function ShowPage(page) {
		// Disable dragging for previous page.
		_popupView.DisableDragging();

		// Save original page values for cancel.
		_currentPage = page;
		_originalPage = $.extend(true, {}, page);

		// Hide previous page contents.
		$getDocumentationView().hide();
		$getDocumentationEditor().hide();
		$getPopupImage().hide();
		_popupView.ClosePage();

		// If there is a current page, show it's contents.
		if (page) {
			if (page.PageType === SH.PageTypes.PopupPage) {
				// Show page image
				$getPopupImage().attr('src', page.DocImageUrl).show();
				_popupView.ShowPage(page);
			} else if (page.PageType === SH.PageTypes.DocumentationPage) {
				// Show custom documentation view form.
				$getDocumentationViewTitle().html(_currentPage.Title);
				$getDocumentationViewContent().html(_currentPage.Content);
				$getDocumentationView().show();
				$getDocumentationEditor().hide();
			}
		}

		InitializeEditTools();
		ResizeDocumentPage();
	}


	// ** Event Handlers **

	// Resets page to original, unedited page.
	function CancelButton_Click() {
		$('#shStatusMessage')
			.html('Changes canceled.')
			.css('background-color', '#3c454f')
			.show()
			.delay(1000)
			.fadeOut('slow');
		ShowPage(_originalPage);
	}

	function EditButton_Click() {
		$getViewModeButtonBar().hide();
		$getEditModeButtonBar().show();
		$getIsHidden().prop('disabled', false);

		if (_currentPage.PageType === SH.PageTypes.PopupPage) {
			$getPopupImageUrl().prop('disabled', false);
			_popupView.EnableDragging($getPageContainer());
		} else if (_currentPage.PageType === SH.PageTypes.DocumentationPage) {
			$getDocumentationEditorTitle().val(_currentPage.Title);
			$getDocumentationEditorContent().val(_currentPage.Content);
			$getDocumentationView().hide();
			$getDocumentationEditor().show();
		}
	}

	function GenerateInstallScriptButton_Click() {
		window.open(SH.Urls.GenerateInstallScript);
	}

	function NewPageButton_Click() {
		var tree = $getTableOfContents().jstree();
		tree.create_node('#', { text: 'New Page' }, 'last', function (new_node) {
			setTimeout(function () { tree.edit(new_node); }, 0);
		});
	}

	function PopupImageUrl_Change() {
		$getPopupImage().attr('src', $(this).val());
		ResizeDocumentPage();
	}

	function ReadTableOfContents_Success(tableOfContents) {

		$.each(tableOfContents, function (index, item) {
			var html = '<li id="shTocEntry-' + item.Id + '" class="shTocEntry' + (item.IsHidden ? ' tocHidden' : '') + '">';
			html += item.Title;
			html += '<ul></ul></li>';

			if (item.ParentPageId) {
				$('#shTocEntry-' + item.ParentPageId + '>ul').append(html);
			} else {
				$getTableOfContents().find('>ul').append(html);
			}
		});

		// Create read only jsTree plugin list and options.
		var plugins = ['wholerow', 'state'];
		var options = {
			'core': {
				'check_callback': true,
				'multiple': false,
				'themes': {
					'icons': false,
				}
			},
			// TODO: Implement types plugin to handle display state of hidden page in TOC
			/*
							'types': {
								'valid_children': "all",
								'types': {
									'visible': {
			
									},
									'hidden': {
			
									},
								},
							},
			*/
			'plugins': plugins,
		};

		// Add edit plugins and their options.
		if (_applicationSettings.CanEdit) {
			plugins.push('contextmenu');
			plugins.push('dnd');

			options.contextmenu = {	// default jstree context menu modified as needed.
				'items': {
					"create": {
						"separator_before": false,
						"separator_after": true,
						"_disabled": false,
						"label": "New Page",
						"action": function (data) {
							var inst = $.jstree.reference(data.reference),
								obj = inst.get_node(data.reference);
							inst.create_node(obj, { text: 'New Page' }, "last", function (new_node) {
								setTimeout(function () { inst.edit(new_node); }, 0);
							});
						}
					},
					"remove": {
						"separator_before": false,
						"icon": false,
						"separator_after": false,
						"_disabled": false,
						"label": "Delete Page",
						"action": function (data) {
							var inst = $.jstree.reference(data.reference),
								node = inst.get_node(data.reference);

							// Move children to parent.
							// Move event handler updates server.
							$.each(node.children, function (index, child) {
								inst.move_node(child, node.parent, 'last');
							});

							// Delete the node in question.
							inst.delete_node(node);
							var pageId = GetPageIdFromElementId(node.id);
							var client = new SH.AjaxClient();
							client.DeletePage(pageId);
						},
					},
				},
			};
		}

		// Initialize read only toc tree.
		var tocTree = $getTableOfContents()
			.jstree(options)
			.on('activate_node.jstree', TocEntry_Activate)
			.on('ready.jstree', Tree_Ready);

		// Add edit mode event handlers.
		if (_applicationSettings.CanEdit) {
			tocTree
				.on('create_node.jstree', TocEntry_Create)
				.on('delete_node.jstree', TocEntry_Delete)
				.on('move_node.jstree', TocEntry_Move)
				.on('rename_node.jstree', TocEntry_Rename);
		}
	}

	// Copy form values to page model and attempt to save page.
	function SaveButton_Click() {
		_currentPage.IsHidden = $getIsHidden().prop('checked');
		if (_currentPage.PageType === SH.PageTypes.DocumentationPage) {
			_currentPage.Title = $getDocumentationEditorTitle().val();
			_currentPage.Content = $getDocumentationEditorContent().val();
		} else if (_currentPage.PageType === SH.PageTypes.PopupPage) {
			_currentPage.DocImageUrl = $getPopupImageUrl().val();
		}
		SavePage();
	}

	function TocEntry_Activate(e, data) {
		// data.node

		ActivateNode(data.node);
	}

	function TocEntry_Create(e, data) {
		// data.node, data.parent, data.position

		var newPage = {
			ParentPageId: GetPageIdFromElementId(data.parent),
			PageType: SH.PageTypes.DocumentationPage,  // All pages created here are Documentation Pages
			Order: data.position,
			Title: 'New Page',
			Content: '',
		};

		var client = new SH.AjaxClient();
		client.SavePage(newPage, function (page) {
			data.instance.set_id(data.node, 'shTocEntry-' + page.Id);
			_newPage = page;
		});
	}

	function TocEntry_Delete(e, data) {
		// data.node, data.parent		

		// Determine new selected node.
		var newSelectedNode = null;
		if (data.node.parent === '#') {	// If the parent was the root element, select the first entry if one exists.
			var $tocEntries = $('.shTocEntry');
			if ($tocEntries.length > 0) {

				// Get the first node id, or second if the first is the deleted node.
				var firstId = $tocEntries.get(0).id;
				if (firstId === data.node.id) {
					firstId = null;
					if ($tocEntries.length > 1)
						firstId = $tocEntries.get(1).id;
				}
				if (firstId)
					newSelectedNode = data.instance.get_node(firstId);
			}
		} else if (data.node.parent) {	// Focus an available parent.
			newSelectedNode = data.instance.get_node(data.node.parent);
		}
		if (newSelectedNode) {	// Only select a new node if one was found.
			data.instance.select_node(newSelectedNode);
			ActivateNode(newSelectedNode, SH.HistoryState.Replace);
		} else // Activating an empty node empties the page.
			ShowPage(null);
	}

	function TocEntry_Move(e, data) {
		// data.node, data.parent, data.position, 
		// data.old_parent, data.old_position, 
		// data.is_nulti, data.old_instance, data.new_instance
		var pageId = GetPageIdFromElementId(data.node.id);
		var newParentId = GetPageIdFromElementId(data.parent);

		var client = new SH.AjaxClient();
		client.MovePage(pageId, newParentId, data.position);

		if (_currentPage.Id === pageId) {
			_currentPage.ParentPageId = newParentId;
			_currentPage.Order = data.position;
		}
	}

	// Method is called when renaming a new page the first time 
	// or when a custom page is edited.
	// Only save the new name when creating a new page.
	function TocEntry_Rename(e, data) {
		if (_newPage) {
			_newPage.Title = data.text;
			var client = new SH.AjaxClient();
			client.SavePage(_newPage);
			_newPage = null;
		}
	}

	function Tree_Ready(e, data) {
		var node = null;

		// Check if a query parameter is set for the id of the page.
		var id = SH.Helpers.GetQueryParameter("id");
		if (id) {
			node = $getTableOfContents().jstree('get_node', 'shTocEntry-' + id);
		}

		// Otherwise, take the first selected node. jsTree is setup to remember the selected node between pages.
		if (!node) {
			var selectedNode = data.instance.get_selected(true);
			if (selectedNode && selectedNode.length > 0) {
				node = selectedNode[0];
			}
		}

		// Load the node if one is present
		if (node)
			ActivateNode(node, SH.HistoryState.Replace);
	}

	function Window_PopState(event) {
		var page = event.state;
		var forward = (page.HistoryId > _historyId);

		// Adjust the history id for this pop event.
		if (forward)
			_historyId++;
		else
			_historyId--;

		var node = $getTableOfContents().jstree('get_node', 'shTocEntry-' + page.Id);
		// If the page from the event state still exists...
		if (node) {
			// Not using ActivateNode because we don't need to read the page model.
			SelectNode(node);
			ShowPage(page);
		} else if (forward)		// Else if the forward button was pressed, move forward in history again.
			history.forward();
		else					// Otherwise, the back button was pressed, so move backward in history again.
			history.back();

		// The above code can result in the same page shown in the history multiple times consecutively.
		// This happens if another page was between them, then was deleted.
		// While this might be confusing from a technical writer's perspective, it is not necessarily 
		//		a bug as the two page models could have different states.
		// As this only affects a very specific fringe case and is hard to solve correctly,
		//		it is not currently slated to be fixed.
	}


	// ** Public Methods **

	// Create dependencies and attach event handlers.
	this.Initialize = function () {
		var options = {
			documentationContainer: $getPageContainer(),	// set to id of documentation container if on documentation page.
			anchoredMode: false,							// true: position elements relative to anchor elements. false: absolute positioning.
			showButtonBar: false,							// set to false to hide bottom button bar.
			showDocumentLink: false,						// set to false to hide title bar documentation link.
		};
		_popupView = new SH.PopupView(options);

		var client = new SH.AjaxClient();
		client.ReadApplicationSettings(function (applicationSettings) {
			_applicationSettings = applicationSettings;
			_popupView.Initialize(applicationSettings);
			InitializeEditTools();
		});
		client.ReadTableOfContents(ReadTableOfContents_Success);

		$getPopupImageUrl().change(PopupImageUrl_Change);
		$getSaveButton().click(SaveButton_Click);
		$getCancelButton().click(CancelButton_Click);
		$getEditButton().click(EditButton_Click);
		$getGenerateInstallScriptButton().click(GenerateInstallScriptButton_Click);
		$getNewPageButton().click(NewPageButton_Click);
		$(window).resize(ResizeDocumentPage);
		window.onpopstate = Window_PopState;
		ResizeDocumentPage();
	}
}

// This is the help button that the external application must have in order to 
// show hidden popup help or the full documentation page.
SH.HelpButton = function () {

	/** Private Fields **/
	var _button = this;
	var _currentPage = null;


	/** HTML Elements **/
	function $getHelpButton() { return $('.shHelpButton'); }

	/** Private Methods **/
	function UpdatePageHelpButton(currentPage) {
		_currentPage = currentPage;

		if (currentPage) {
			$getHelpButton().attr('Title', 'Show help for this page');
		} else {
			$getHelpButton().attr('Title', 'View documentation');
		}
	}


	/** Event Handlers **/
	function HelpButton_Click() {
		if (_currentPage)
			$(_button).trigger('showPopup.sh');
		else
			window.location = SH.Urls.NavigateDocumentation;
	}


	/** Public Methods **/
	this.Initialize = function (popupEdit, currentPage) {
		UpdatePageHelpButton(currentPage);

		$(popupEdit).bind("pageUpdate.sh", function (event, newPage) {
			UpdatePageHelpButton(newPage);
		});

		$getHelpButton().click(HelpButton_Click);
	}


	/** Events **/
	// showPopup.sh
}

SH.PopupEdit = function () {

	// ** Private Fields **
	var _anchoredElementLogic = new SH.AnchoredElementLogic();
	var _applicationSettings = null;
	var _currentPage = null;
	var _elements = new SH.SharedElements();
	var _leafElements = null;
	var _originalPage = null;
	var _self = this;


	/** HTML Elements **/
	function $getAddBulletButton() { return $('#shAddBulletButton'); }
	function $getBulletEdit(context) { return $('.shBulletEdit', context); }
	function $getBulletEditTemplate() { return $('#shBulletEditTemplate'); }
	function $getBulletEditText(context) { return $('.shBulletEditText', context); }
	function $getCancelHelpButton() { return $('#shCancelHelpButton'); }
	function $getCreateHelpButton() { return $('#shCreateHelpButton'); }
	function $getDialogTitle() { return $('.shDialogTitle'); }
	function $getDraggableBullet(context) { return $('>.shDraggableBullet', context); }
	function $getDraggableBulletNumber(context) { return $('.shDraggableBulletNumber', context); }
	function $getPopupEdit() { return $('#shPopupEdit'); }
	function $getPopupEditBullets() { return $('#shPopupEditBullets'); }
	function $getPopupEditContent() { return $('#shPopupEditContent'); }
	function $getPopupEditMatchUrls() { return $('#shPopupEditMatchUrls'); }
	function $getPopupEditTitle() { return $('#shPopupEditTitle'); }
	function $getSaveHelpButton() { return $('#shSaveHelpButton'); }


	// ** Private Methods **
	function AddBullet(bullet) {
		// Clone the template to create a new editor, initialize, and add to form.
		var $newBullet = $getBulletEditTemplate()
			.clone(true, true)
			.attr('id', 'shBulletEdit-' + bullet.Number);
		$newBullet.find('.shBulletNumber').html(bullet.Number);
		if (bullet.Text) {
			$getBulletEditText($newBullet).val(bullet.Text);
		}
		$getPopupEditBullets().append($newBullet);

		// Clone the draggable bullet template, initilize, and add to page.
		var $draggableBullet = $getDraggableBullet($newBullet);
		$getDraggableBulletNumber($draggableBullet).html(bullet.Number);
		$draggableBullet.detach();
		$('body').append($draggableBullet);
		var bulletTop;
		var bulletLeft;
		if (bullet.OffsetElementId) {
			var anchorOffset = $('#' + bullet.OffsetElementId).offset();
			bulletTop = anchorOffset.top + bullet.VerticalOffset;
			bulletLeft = anchorOffset.left + bullet.HorizontalOffset;
		} else {
			bulletTop = $newBullet.offset().top - 3;
			bulletLeft = $newBullet.offset().left - 50;
		}
		$draggableBullet.attr('id', 'shDraggableBullet-' + bullet.Number)
			.css({ top: bulletTop + 'px', left: bulletLeft + 'px' })
			.draggable({
				containment: 'document',
				stop: DraggableBullet_DraggableStop,
			});
		UpdateBulletPositionValues($draggableBullet.get(0));
	}

	function CloseEditHelpForm() {
		$getDraggableBullet('body').remove();
		$getPopupEdit().hide();
		_elements.$getPopupMask().hide();
		$getCreateHelpButton().show();
	}

	function InitializeCreateMode() {
		$getSaveHelpButton().html('Create');
		$getDialogTitle().html('Create Help Page');

		$getPopupEditTitle().val('Help - ' + document.title);
		$getPopupEditMatchUrls().val(window.location.pathname);
		$getPopupEditContent().val('');
		$getPopupEditBullets().empty();

		var dialogLeft = ($(document).width() / 3 * 2) - ($getPopupEdit().width());
		$getPopupEdit().css({ top: '200px', left: dialogLeft });
	}

	function InitializeEditMode(page) {
		$getSaveHelpButton().html('Save');
		$getDialogTitle().html('Edit Help Page');

		$getPopupEditTitle().val(page.Title);
		$getPopupEditMatchUrls().val(page.MatchUrls);
		$getPopupEditContent().val(page.Content);

		$getPopupEditBullets().empty();
		$.each(page.Bullets, function (index, bullet) {
			AddBullet(bullet);
		});

		var anchor = $('#' + page.OffsetElementId);
		var top = (anchor.offset().top + page.VerticalOffset) + 'px';
		var left = (anchor.offset().left + page.HorizontalOffset) + 'px';
		$getPopupEdit().css({ top: top, left: left });
	}

	// Find the LeafElement that overlaps most with, 
	// or is closest to the dropped element.
	function GetAnchor(dropElement) {
		var $dropElement = $(dropElement);
		var dropLeft = $dropElement.offset().left;
		var dropRight = dropLeft + $dropElement.width();
		var dropX = (dropRight - dropLeft) / 2 + dropLeft;
		var dropTop = $dropElement.offset().top;
		var dropBottom = dropTop + $dropElement.height();
		var dropY = (dropBottom - dropTop) / 2 + dropTop;

		var result = null;
		_leafElements.each(function (index, leafElement) {
			var $leafElement = $(leafElement);
			var leafLeft = $leafElement.offset().left;
			var leafRight = leafLeft + $leafElement.width();
			var leafX = (leafRight - leafLeft) / 2 + leafLeft;
			var leafTop = $leafElement.offset().top;
			var leafBottom = leafTop + $leafElement.height();
			var leafY = (leafBottom - leafTop) / 2 + leafTop;

			var distance = Math.sqrt(Math.pow(Math.abs(dropX - leafX), 2) + Math.pow(Math.abs(dropY - leafY), 2));

			if (!result) {
				result = {
					Distance: distance,
					HorizontalOffset: Math.round(dropLeft - leafLeft),
					VerticalOffset: Math.round(dropTop - leafTop),
					OffsetElementId: $leafElement.attr('id'),
				};
			}
			else {
				if (distance < result.Distance) {
					result = {
						Distance: distance,
						HorizontalOffset: Math.round(dropLeft - leafLeft),
						VerticalOffset: Math.round(dropTop - leafTop),
						OffsetElementId: $leafElement.attr('id'),
					};
				}
			}
		});
		return result;
	}

	function UpdateBulletPositionValues(element) {
		var anchor = GetAnchor(element);
		var number = parseInt($getDraggableBulletNumber($(element)).html());

		// Find the bullet model for the element.		
		var bullet;
		$.each(_currentPage.Bullets, function (index, loopBullet) {
			if (loopBullet.Number === number)
				bullet = loopBullet;
		});

		bullet.HorizontalOffset = anchor.HorizontalOffset;
		bullet.VerticalOffset = anchor.VerticalOffset;
		bullet.OffsetElementId = anchor.OffsetElementId;
	}

	function UpdateFormPositionValues(element) {
		var anchor = GetAnchor(element);
		_currentPage.HorizontalOffset = anchor.HorizontalOffset;
		_currentPage.VerticalOffset = anchor.VerticalOffset;
		_currentPage.OffsetElementId = anchor.OffsetElementId;
	}

	function UpdatePageModel() {
		var href = window.location.href;
		var host = window.location.host;
		var sourceUrl = href.substring(href.indexOf(host) + host.length, href.length);

		_currentPage.Id = _currentPage.Id || 0;
		_currentPage.PageType = _currentPage.PageType || SH.PageTypes.PopupPage;
		_currentPage.Title = $getPopupEditTitle().val();
		_currentPage.MatchUrls = $getPopupEditMatchUrls().val();
		_currentPage.SourceUrl = sourceUrl;
		_currentPage.Content = $getPopupEditContent().val();

		$.each(_currentPage.Bullets, function (index, bullet) {
			bullet.Text = $getBulletEditText($('#shBulletEdit-' + (index + 1))).val();
		});
	}


	// ** Event Handlers **
	// if bullet is null, create new bullet. else, populate with existing data.
	function AddBulletButton_Click() {
		var bullet = {
			Number: _currentPage.Bullets.length + 1,
		};
		_currentPage.Bullets.push(bullet);

		AddBullet(bullet);
	}

	function CancelHelpButton_Click() {
		_currentPage = _originalPage;
		CloseEditHelpForm();
	}

	function CreateHelpButton_Click() {
		// If there isn't a page, don't extend the empty object, otherwise...
		// Deep copy current page model to place holder so that edits may be canceled.
		if (_originalPage)
			_originalPage = $.extend(true, {}, _currentPage);

		if (_currentPage) {
			InitializeEditMode(_currentPage);
		} else {
			_currentPage = {
				Bullets: new Array(),
			};
			InitializeCreateMode();
		}

		// flip visibilities and position form.
		$getCreateHelpButton().hide();
		_elements.$getPopupMask().show();

		var $form = $getPopupEdit().show();
		UpdateFormPositionValues($form.get(0));
	}

	function DraggableBullet_DraggableStop() {
		UpdateBulletPositionValues(this);
		UpdatePageModel();
	}

	function EditForm_DraggableStop() {
		UpdateFormPositionValues(this);
		UpdatePageModel();
	}

	function RemoveBulletButton_Click() {
		// Remove the editor and draggable bullet associated with this remove button.
		var $editor = $(this).closest('.shBulletEdit');
		var number = $editor.find('.shBulletNumber').html();
		$editor.remove();
		$('#shDraggableBullet-' + number).remove();

		// Reassign all bullet numbers and element ids from top down.
		$getBulletEdit($getPopupEditBullets()).each(function (index, editor) {
			var newNumber = index + 1;
			var $currentNumber = $(editor).find('.shBulletNumber');
			var currentNumber = $currentNumber.html();
			$currentNumber.html(newNumber);
			$(editor).attr('id', 'shBulletEdit-' + newNumber);
			var $draggableBullet = $('#shDraggableBullet-' + currentNumber).attr('id', 'shDraggableBullet-' + newNumber);
			$getDraggableBulletNumber($draggableBullet).html(newNumber);
		});

		// Remove bullet from model.
		_currentPage.Bullets.splice(number - 1, 1);

		// Reassign numbers on remaining models.
		$.each(_currentPage.Bullets, function (index, bullet) {
			bullet.Number = index + 1;
		});
	}

	function SaveHelpButton_Click() {
		UpdatePageModel();
		var client = new SH.AjaxClient();
		client.SavePage(_currentPage, function (page) {
			_currentPage.Id = page.Id;
		});

		$getCreateHelpButton().html('Edit Help Page');
		CloseEditHelpForm();

		$(_self).trigger('pageUpdate.sh', _currentPage);
	}

	function Window_Resize() {
		_anchoredElementLogic.UpdateElementPositions(_currentPage, $getPopupEdit());
	}


	// ** Public Methods ** 
	this.Initialize = function (applicationSettings, page) {
		_applicationSettings = applicationSettings;
		_currentPage = page;

		if (_applicationSettings && _applicationSettings.CanEdit) {
			if (_currentPage)
				$getCreateHelpButton().html('Edit Help Page');

			// Initializes the form as much as possible.
			$getCreateHelpButton().click(CreateHelpButton_Click);
			$getSaveHelpButton().click(SaveHelpButton_Click);
			$getCancelHelpButton().click(CancelHelpButton_Click);
			$getAddBulletButton().click(AddBulletButton_Click);
			$getBulletEditTemplate().find('.shButtonCancel').click(RemoveBulletButton_Click);
			$getPopupEdit().draggable({
				handle: $getDialogTitle(),
				containment: 'document',
				stop: EditForm_DraggableStop,
			});
			// for drop anchors, only interested in visible elements that have ids, 
			// contain no child nodes, are not help related and are not certain other tags.
			_leafElements = $('*[id]:visible:not(.sh-,.sh- *,link,meta,script,style,:has(*))');
		}

		$(window).resize(Window_Resize);
	}


	// ** Events ** 
	// pageUpdate.sh
}

SH.PopupView = function (options) {

	/** Default Options **/
	var _options = {
		documentationContainer: null,	// set to id of documentation container if on documentation page.
		anchoredMode: true,				// true: position elements relative to anchor elements. false: absolute positioning.
		showButtonBar: true,			// set to false to hide bottom button bar.
		showDocumentLink: true,			// set to false to hide title bar documentation link.
	};
	if (options) { $.extend(_options, options); }


	// ** Private Fields **
	var _anchoredElementLogic = new SH.AnchoredElementLogic();
	var _applicationSettings = null;
	var _currentPage = null;
	var _dragablesInitialized = false;
	var _elements = new SH.SharedElements();
	var _self = this;
	var _userPageSettings = null;
	var _nextBulletOffset = 0;		// Tracks left offset for unset bullets on documentation page.


	/** HTML Elements **/
	function $getBulletNumber(context) { return $('.shBulletNumber', context); }
	function $getBulletViewTemplate() { return $('#shBulletViewTemplate'); }
	function $getBulletViewText(context) { return $('.shBulletViewText', context); }
	function $getButtonBar() { return $('#shPopupView .shButtonBar'); }
	function $getCloseHelpButton() { return $('#shCloseHelpButton'); }
	function $getDialogTitle() { return $('.shDialogTitle'); }
	function $getHideHelpButton() { return $('#shHideHelpButton'); }
	function $getPlacedBulletNumber(context) { return $('.shPlacedBulletNumber', context); }
	function $getPlacedBullets(context) { return $('>.shPlacedBullet', context); }
	function $getPopupView() { return $('#shPopupView'); }
	function $getPopupViewBullets() { return $('#shPopupViewBullets'); }
	function $getPopupViewContent() { return $('#shPopupViewContent'); }
	function $getShowDocumentationButton() { return $('#shShowDocumentationButton'); }


	// ** Private Methods **
	function AjaxSaveUserPageSettings(hidePage) {
		if (null === _userPageSettings) {
			_userPageSettings = {
				PageId: _currentPage.Id,
				HidePage: hidePage,
			};
		} else {
			_userPageSettings.HidePage = hidePage;
		}

		var client = new SH.AjaxClient();
		client.SaveUserPageSettings(_userPageSettings);
	}

	function InitializeDraggables() {
		if (_options.documentationContainer
			&& _applicationSettings
			&& _applicationSettings.CanEdit
			&& _currentPage
			&& _currentPage.PageType === SH.PageTypes.PopupPage) {

			$getDialogTitle().css({ cursor: 'move' });
			$getPopupView().draggable({
				handle: $getDialogTitle(),
				containment: _options.documentationContainer,
				stop: PopupView_DraggableStop,
			});
			$getPlacedBullets(_options.documentationContainer).each(function (index, bullet) {
				$(bullet).css({ cursor: 'move' })
					.draggable({
						containment: _options.documentationContainer,
						stop: Bullet_DraggableStop,
					});
			});

			_dragablesInitialized = true;
		}
	}

	function PlaceBullet(bullet) {
		var $container, top, left;
		if (_options.anchoredMode) {
			$container = $('body');
			var anchorOffset = $('#' + bullet.OffsetElementId).offset();
			top = anchorOffset ? anchorOffset.top + bullet.VerticalOffset : 0;
			left = anchorOffset ? anchorOffset.left + bullet.HorizontalOffset : _nextBulletOffset;
		} else {
			$container = _options.documentationContainer;
			top = bullet.DocVerticalOffset || 0;
			left = bullet.DocHorizontalOffset || _nextBulletOffset;

			_nextBulletOffset += 40;
		}

		var $newBullet = $getBulletViewTemplate()
			.clone(true, true)
			.attr("id", 'shBulletView-' + bullet.Number);
		$getBulletNumber($newBullet).html(bullet.Number);
		$getBulletViewText($newBullet).html(bullet.Text);

		$getPopupViewBullets().append($newBullet);

		var placedBullet = $getPlacedBullets($newBullet);
		$getPlacedBulletNumber(placedBullet).html(bullet.Number);
		placedBullet.detach();
		$container.append(placedBullet);
		placedBullet.attr('id', 'shPlacedBullet-' + bullet.Number)
			.css({ top: top, left: left });
	}

	function RemoveBullets() {
		var $container = _options.documentationContainer || $('body');
		$getPlacedBullets($container).remove();
	}


	// ** Event Handlers **
	function Bullet_DraggableStop() {
		var bullets = _currentPage.Bullets;
		var $bullet = $(this);
		var number = parseInt($getPlacedBulletNumber($bullet).html());
		$.each(bullets, function (index, bullet) {
			if (bullet.Number === number) {
				bullet.DocVerticalOffset = Math.round($bullet.css('top').replace('px', ''));
				bullet.DocHorizontalOffset = Math.round($bullet.css('left').replace('px', ''));
			}
		});
	}

	function CloseHelpButtons_Click(e) {
		e.data.model.ClosePage();
		var cookies = new SH.Cookies();
		cookies.SetCookie("SH-HideHelp", e.data.hide.toString(), 730);
		AjaxSaveUserPageSettings(e.data.hide);
	}

	function PopupView_DraggableStop() {
		_currentPage.DocVerticalOffset = Math.round($(this).css('top').replace('px', ''));
		_currentPage.DocHorizontalOffset = Math.round($(this).css('left').replace('px', ''));
	}

	function ViewHelpButton_Click() {
		// TODO: move documentation button logic out of PopupView.
		// will need to tease apart new page creation events... maybe.
		// popupView defined in PopupViewInjectedScripts.html
		popupView.ShowPage();
	}

	function Window_Resize() {
		if (_options.anchoredMode)
			_anchoredElementLogic.UpdateElementPositions(_currentPage, $getPopupView());
	}


	// ** Public Methods **
	this.ClosePage = function () {
		RemoveBullets();
		$getPopupView().hide();
		_elements.$getPopupMask().hide();
	}

	this.DisableDragging = function () {
		if (_dragablesInitialized) {
			$getDialogTitle().css({ cursor: 'auto' });
			// Must destroy draggables because the next page shown may have a different number of elements.
			$getPopupView().draggable('destroy');
			$getPlacedBullets(_options.documentationContainer).each(function (index, bullet) {
				$(bullet).css({ cursor: 'auto' })
					.draggable('destroy');
			});
			_dragablesInitialized = false;
		}
	}

	this.EnableDragging = function () {
		InitializeDraggables();
	}

	this.Initialize = function (applicationSettings, popupEdit, helpButton) {
		_applicationSettings = applicationSettings;

		if (_options.showButtonBar) {
			$getCloseHelpButton().click({ model: this, hide: false }, CloseHelpButtons_Click);
			$getHideHelpButton().click({ model: this, hide: true }, CloseHelpButtons_Click);
		} else {
			$getButtonBar().hide();
		}

		if (_options.showDocumentLink) {
			$getShowDocumentationButton()
				.show()
				.click(function () { window.location = SH.Urls.NavigateDocumentation; });
		} else {
			$getShowDocumentationButton().hide();
		}

		$(window).resize(Window_Resize);

		$(popupEdit).bind("pageUpdate.sh", function (event, newPage) {
			_currentPage = newPage;
		});
		$(helpButton).bind("showPopup.sh", function () {
			_self.ShowPage();
		});
	}

	this.ShowPage = function (page) {
		// Use previous page if not present.
		if (page)
			_currentPage = page;

		if (_currentPage) {
			// This is needed on the documentation page when moving from one popup help to another popup help.
			// But it doesn't hurt to have for all other transitions.
			RemoveBullets();

			$getPopupViewContent().html(_currentPage.Content);
			$getDialogTitle().html(_currentPage.Title);

			var view = $getPopupView();
			if (_options.anchoredMode) {
				_anchoredElementLogic.UpdateElementPosition(_currentPage, view);
			} else {
				var formTop = page.DocVerticalOffset || 40;
				var formLeft = page.DocHorizontalOffset || 0;
				view.show().css({ top: formTop + 'px', left: formLeft + 'px' });
			}

			_nextBulletOffset = 0;
			$getPopupViewBullets().empty();
			$.each(_currentPage.Bullets, function (index, bullet) {
				PlaceBullet(bullet);
			});

			_elements.$getPopupMask().show();
			view.show();
		}
	}

	this.ShowPageFirstTime = function (page, userPageSettings) {
		_userPageSettings = userPageSettings;
		_currentPage = page;

		var cookies = new SH.Cookies();

		if (_currentPage &&
			cookies.GetCookie("SH-HideHelp") !== "true" &&
			(_userPageSettings === null || !_userPageSettings.HidePage)) {
			this.ShowPage();
		}
	}
}

SH.SharedElements = function () {
	/** HTML Elements **/
	this.$getPopupMask = function () { return $('#shPopupMask'); }
}
