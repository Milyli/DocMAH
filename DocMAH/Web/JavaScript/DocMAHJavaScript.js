/** Namespace **/
var DMH = DMH || {};
var docmahJQuery = $.noConflict(true);

DMH.HistoryState = function () {
	this.Replace = 1;
	this.Push = 2;
	this.Pop = 3;

	return this;
}();

DMH.Urls = function () {

	// ** Public Fields **
	this.DeletePage = '/help.axd?m=DeletePage';
	this.GenerateInstallScript = '/help.axd?m=GenerateInstallScript';
	this.MovePage = '/help.axd?m=MovePage';
	this.NavigateDocumentation = '/help.axd?m=DocumentationPage';
	this.ReadApplicationSettings = '/help.axd?m=ReadApplicationSettings';
	this.ReadPage = '/help.axd?m=ReadPage';
	this.ReadTableOfContents = '/help.axd?m=ReadTableOfContents';
	this.SaveDocumentationPage = '/help.axd?m=SaveDocumentationPage';
	this.SaveFirstTimeHelp = '/help.axd?m=SaveFirstTimeHelp';
	this.SaveUserPageSettings = '/help.axd?m=SaveUserPageSettings';

	// Return singleton.
	return this;
}();

DMH.Helpers = function () {
	this.GetQueryParameter = function (name) {
		name = name.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
		var regex = new RegExp("[\\?&]" + name + "=([^&#]*)"),
			results = regex.exec(location.search);
		return results === null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
	}

	return this;
}();

DMH.AjaxClient = function () {
	var $ = docmahJQuery;

	// ** Public Methods **
	this.DeletePage = function (pageId, successCallback) {
		$.ajax({
			type: 'POST',
			url: DMH.Urls.DeletePage,
			data: pageId.toString(),
			success: successCallback,
		});
	}

	this.MovePage = function (pageId, newParentId, newPosition) {
		$.ajax({
			type: 'POST',
			url: DMH.Urls.MovePage,
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
			url: DMH.Urls.ReadApplicationSettings,
			success: successCallback,
			async: false,
		});
	}

	this.ReadPage = function (id, successCallback) {
		$.ajax({
			type: 'GET',
			data: { id: id },
			url: DMH.Urls.ReadPage,
			success: successCallback,
		});
	}

	this.ReadTableOfContents = function (successCallback) {
		$.ajax({
			type: 'GET',
			url: DMH.Urls.ReadTableOfContents,
			success: successCallback,
		});
	}

	this.SaveDocumentationPage = function (page, successCallback) {
		$.ajax({
			'type': 'POST',
			'contentType': 'application/json',
			'dataType': 'json',
			'url': DMH.Urls.SaveDocumentationPage,
			'data': JSON.stringify(page),
			'success': successCallback,
		});
	}

	this.SaveFirstTimeHelp = function (help, successCallback) {
		$.ajax({
			type: 'POST',
			contentType: "application/json",
			dataType: 'json',
			url: DMH.Urls.SaveFirstTimeHelp,
			data: JSON.stringify(help),
			success: successCallback,
		});
	}

	this.SaveUserPageSettings = function (userPageSettings) {
		$.ajax({
			type: 'POST',
			contentType: "application/json",
			dataType: 'json',
			url: DMH.Urls.SaveUserPageSettings,
			data: JSON.stringify(userPageSettings),
		});
	}
}

DMH.AnchoredElementLogic = function () {
	var self = this;
	var _elements = new DMH.SharedElements();
	var $ = docmahJQuery;

	// ** Public Methods **
	this.UpdateElementPosition = function (model, $element) {
		if ($element.length > 0) {
			var anchorOffset = $('#' + model.OffsetElementId).offset();
			var top = anchorOffset ? anchorOffset.top + model.VerticalOffset : 0;
			var left = anchorOffset ? anchorOffset.left + model.HorizontalOffset : 0;

			// Check the right first, then always enforce the left even if it pushes off the right again.
			// I'd. rather see the left than the right.
			if (left + $element.width() > $(document).width())
				left = $(document).width() - $element.width();
			if (left < 0)
				left = 0;

			// Check the bottom first, then always enforce the top even if it pushes off the bottom again.
			// I'd. rather see the top than the bottom.
			if (top + $element.height() > $(document).height())
				top = $(document).height() - $element.height();
			if (top < 0)
				top = 0;

			$element.css({ top: top, left: left, });
		}
	}

	this.UpdateElementPositions = function (pageModel, $form) {
		if (_elements.$getModalMask().is(':visible')) {
			$.each(pageModel.Bullets, function (index, bullet) {
				self.UpdateElementPosition(bullet, $('#dmhDraggableBullet-' + bullet.Number));
				self.UpdateElementPosition(bullet, $('#dmhPlacedBullet-' + bullet.Number));
			});
			if ($form.is(':visible')) {
				self.UpdateElementPosition(pageModel, $form);
			}
		}
	}
}

DMH.Cookies = function () {

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

DMH.DocumentationPage = function () {

	// ** Private Fields **
	var _applicationSettings = null;
	var _currentPage = null;
	var _newPage = null;		// Temporary new page when tree creates new page and name needs to be saved.
	var _originalPage = null;	// State of page before editing begins.
	var _historyId = 1;			// Compared to popstate data to determine if forward or back was pressed.
	var $ = docmahJQuery;


	/** HTML Elements **/
	function $getCancelButton() { return $('#dmhCancelButton'); }
	function $getContentPanel() { return $('#dmhDocContentPanel'); }
	function $getDeleteButton() { return $('#dmhDeletePageButton'); }
	function $getDocumentationEditor() { return $('#dmhDocumentationEditor'); }
	function $getDocumentationEditorContent() { return $('#dmhDocumentationEditorContent'); }
	function $getDocumentationEditorTitle() { return $('#dmhDocumentationEditorTitle'); }
	function $getDocumentationView() { return $('#dmhDocumentationView'); }
	function $getDocumentationViewContent() { return $('#dmhDocumentationViewContent'); }
	function $getDocumentationViewTitle() { return $('#dmhDocumentationViewTitle'); }
	function $getEditButton() { return $('#dmhEditPageButton'); }
	function $getEditModeButtonBar() { return $('#dmhEditModeButtonBar'); }
	function $getEditToolBar() { return $('#dmhEditToolBar'); }
	function $getGenerateInstallScriptButton() { return $('#dmhGenerateInstallScriptButton'); }
	function $getIsHidden() { return $('#dmhIsHidden'); }
	function $getLeftContainer() { return $('#dmhLeftContainer'); }
	function $getNewPageButton() { return $('#dmhNewPageButton'); }
	function $getPageContainer() { return $('#dmhPageContainer'); }
	function $getRightContainer() { return $('#dmhRightContainer'); }
	function $getSaveButton() { return $('#dmhSaveButton'); }
	function $getTableOfContents() { return $('#dmhToc'); }
	function $getTitle() { return $('#dmhDocTitle'); }
	function $getViewModeButtonBar() { return $('#dmhViewModeButtonBar'); }


	// ** Private Methods **

	// Reads the page model for the given node, selects the node in the toc and shows the page model.
	function ActivateNode(node, historyState) {
		historyState = historyState || DMH.HistoryState.Push;

		var id = GetPageIdFromElementId(node.id);

		// if the clicked toc node id is valid (this is not a new page) and no page is loaded 
		//	or the node clicked does not represent the current page, request the page information.
		if (id && (!(_currentPage) || (_currentPage.Id !== id))) {
			var client = new DMH.AjaxClient();
			client.ReadPage(id, function (page) {

				// Handle browser history as needed.
				var url = "help.axd?m=DocumentationPage&id=" + page.Id;
				if (historyState === DMH.HistoryState.Replace) {
					page.HistoryId = _historyId;
					history.replaceState(page, page.Title, url);
				} else if (historyState === DMH.HistoryState.Push) {
					page.HistoryId = ++_historyId;
					history.pushState(page, page.Title, url);
				} else if (historyState === DMH.HistoryState.Pop) {
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
		if (elementId === '#')
			return null;
		return parseInt(elementId.substring("dmhTocEntry-".length));
	}

	// Sets edit tool bar to initial values and view mode for current page.
	function InitializeEditTools() {
		if (_applicationSettings &&
			_applicationSettings.CanEdit) {

			// Reset all bars.
			$getDeleteButton().hide();
			$getEditToolBar().hide();
			$getEditModeButtonBar().hide();
			$getEditButton().hide();
			$getViewModeButtonBar().show();

			// If there is a current page, show the editor for the current page type.
			if (_currentPage) {
				$getDeleteButton().show();
				$getEditButton().show();
				$getIsHidden()
					.prop('disabled', true)
					.prop('checked', _currentPage.IsHidden);
			}

			$getEditToolBar().show();
			var editToolBarHeight = $getEditToolBar().innerHeight() + 'px';
			$getLeftContainer().css('bottom', editToolBarHeight);
			$getRightContainer().css('bottom', editToolBarHeight);
		}
	}

	// Attempt to persist page model to server.
	// Reset the document to view mode for current page if successful.
	function SavePage() {
		if (_currentPage) {
			var client = new DMH.AjaxClient();
			client.SaveDocumentationPage(_currentPage, function () {

				var node = $getTableOfContents().jstree('get_node', 'dmhTocEntry-' + _currentPage.Id);

				// Update table of contents UI as needed for each type.
				$getTableOfContents().jstree('rename_node', node, _currentPage.Title);

				// Set UI visibility indicator on current node. 
				// CSS handles changing style on children.
				if (_currentPage.IsHidden) {
					node.li_attr['class'] = 'tocHidden';
					$(('#' + node.id)).addClass('tocHidden');
				} else {
					node.li_attr['class'] = '';
					$(('#' + node.id)).removeClass('tocHidden');
				}

				ShowMessage('Page saved.', 'success');
				ShowPage(_currentPage);
			});
		}
	}

	function SelectNode(node) {
		var tree = $getTableOfContents().jstree();
		var clearNodes = tree.get_selected(true);
		tree.deselect_node(clearNodes);
		tree.select_node(node);
	}

	function ShowMessage(message, type) {
		var style = 'dmhStatusWarning';

		if (type === 'success')
			style = 'dmhStatusSuccess';

		var $message = $('#dmhStatusMessage');
		$message.html(message)
			.addClass(style)
			.show()
			.delay(2000)
			.fadeOut('slow', function () { $message.removeClass(style); });
	}

	// Sets up the content area for the current page type.
	function ShowPage(page) {
		// Save original page values for cancel.
		_currentPage = page;
		_originalPage = $.extend(true, {}, page);

		// Hide previous page contents.
		$getDocumentationView().hide();
		$getDocumentationEditor().hide();

		// If there is a current page, show it's contents.
		if (page) {
			// Show custom documentation view form.
			$getDocumentationViewTitle().html(_currentPage.Title);
			$getDocumentationViewContent().html(_currentPage.Content);
			$getDocumentationView().show();
			$getDocumentationEditor().hide();
		}

		InitializeEditTools();
	}


	// ** Event Handlers **

	// Resets page to original, unedited page.
	function CancelButton_Click() {
		ShowMessage('Changes canceled.', 'warning');
		ShowPage(_originalPage);
	}

	function DeleteButton_Click() {
		if (_currentPage) {
			var pageId = _currentPage.Id;

			var tree = $getTableOfContents().jstree();
			var node = tree.get_node('dmhTocEntry-' + pageId);

			// Move children to parent.
			// Move event handler updates server.
			$.each(node.children, function (index, child) {
				tree.move_node(child, node.parent, 'last');
			});

			// Delete the node.
			tree.delete_node(node);

			var client = new DMH.AjaxClient();
			client.DeletePage(pageId, function () { ShowMessage('Page deleted.', 'success'); });
		}
	}

	function EditButton_Click() {
		$getViewModeButtonBar().hide();
		$getEditModeButtonBar().show();
		$getIsHidden().prop('disabled', false);

		$getDocumentationEditorTitle().val(_currentPage.Title);
		$getDocumentationEditorContent().val(_currentPage.Content);
		$getDocumentationView().hide();
		$getDocumentationEditor().show();
	}

	function GenerateInstallScriptButton_Click() {
		window.open(DMH.Urls.GenerateInstallScript);
	}

	function NewPageButton_Click() {
		var tree = $getTableOfContents().jstree();
		tree.create_node('#', { text: 'New Page' }, 'last', function (new_node) {
			ShowMessage('Page created.', 'success');

			setTimeout(function () { tree.edit(new_node); }, 0);
		});
	}

	function ReadTableOfContents_Success(tableOfContents) {

		$.each(tableOfContents, function (index, item) {
			var html = '<li id="dmhTocEntry-' + item.Id + '" class="dmhTocEntry' + (item.IsHidden ? ' tocHidden' : '') + '">';
			html += item.Title;
			html += '<ul></ul></li>';

			if (item.ParentPageId) {
				$('#dmhTocEntry-' + item.ParentPageId + '>ul').append(html);
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
			'plugins': plugins,
		};

		// Add edit plugins and their options.
		if (_applicationSettings.CanEdit) {
			plugins.push('dnd');
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
		_currentPage.Title = $getDocumentationEditorTitle().val();
		_currentPage.Content = $getDocumentationEditorContent().val();
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
			Order: data.position,
			Title: 'New Page',
			Content: '',
		};

		var client = new DMH.AjaxClient();
		client.SaveDocumentationPage(newPage, function (page) {
			data.instance.set_id(data.node, 'dmhTocEntry-' + page.Id);
			_newPage = page;
		});
	}

	function TocEntry_Delete(e, data) {
		// data.node, data.parent		

		// Determine new selected node.
		var newSelectedNode = null;
		if (data.node.parent === '#') {	// If the parent was the root element, select the first entry if one exists.
			var $tocEntries = $('.dmhTocEntry');
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
			ActivateNode(newSelectedNode, DMH.HistoryState.Replace);
		} else // Activating an empty node empties the page.
			ShowPage(null);
	}

	function TocEntry_Move(e, data) {
		// data.node, data.parent, data.position, 
		// data.old_parent, data.old_position, 
		// data.is_nulti, data.old_instance, data.new_instance
		var pageId = GetPageIdFromElementId(data.node.id);
		var newParentId = GetPageIdFromElementId(data.parent);

		var client = new DMH.AjaxClient();
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
			var client = new DMH.AjaxClient();
			client.SaveDocumentationPage(_newPage, function () { ShowMessage('Page renamed.', 'success'); });
			_newPage = null;
		}
	}

	function Tree_Ready(e, data) {
		var node = null;

		// Check if a query parameter is set for the id of the page.
		var id = DMH.Helpers.GetQueryParameter("id");
		if (id) {
			node = $getTableOfContents().jstree('get_node', 'dmhTocEntry-' + id);
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
			ActivateNode(node, DMH.HistoryState.Replace);
	}

	function Window_PopState(event) {
		var page = event.state;
		var forward = (page.HistoryId > _historyId);

		// Adjust the history id for this pop event.
		if (forward)
			_historyId++;
		else
			_historyId--;

		var node = $getTableOfContents().jstree('get_node', 'dmhTocEntry-' + page.Id);
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
		var client = new DMH.AjaxClient();
		client.ReadApplicationSettings(function (applicationSettings) {
			_applicationSettings = applicationSettings;
			InitializeEditTools();
		});
		client.ReadTableOfContents(ReadTableOfContents_Success);

		$getCancelButton().click(CancelButton_Click);
		$getDeleteButton().click(DeleteButton_Click);
		$getEditButton().click(EditButton_Click);
		$getGenerateInstallScriptButton().click(GenerateInstallScriptButton_Click);
		$getNewPageButton().click(NewPageButton_Click);
		$getSaveButton().click(SaveButton_Click);
		window.onpopstate = Window_PopState;
	}
}

// This is the help button that the external application must have in order to 
// show hidden first time help or the full documentation page.
DMH.HelpButton = function () {

	/** Private Fields **/
	var _button = this;
	var _currentPage = null;
	var $ = docmahJQuery;

	/** HTML Elements **/
	function $getHelpButton() { return $('.dmhHelpButton'); }

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
			$(_button).trigger('showFirstTime.dmh');
		else
			window.open(DMH.Urls.NavigateDocumentation, '_blank');
	}


	/** Public Methods **/
	this.Initialize = function (firstTimeEdit, currentPage) {
		UpdatePageHelpButton(currentPage);

		$(firstTimeEdit).bind("pageUpdate.dmh", function (event, newPage) {
			UpdatePageHelpButton(newPage);
		});

		$getHelpButton().click(HelpButton_Click);
	}


	/** Events **/
	// showFirstTime.dmh
}

DMH.FirstTimeEdit = function () {

	// ** Private Fields **
	var _anchoredElementLogic = new DMH.AnchoredElementLogic();
	var _applicationSettings = null;
	var _currentPage = null;
	var _elements = new DMH.SharedElements();
	var _leafElements = null;
	var _originalPage = null;
	var _self = this;
	var $ = docmahJQuery;


	/** HTML Elements **/
	function $getAddBulletButton() { return $('#dmhAddBulletButton'); }
	function $getBulletEdit(context) { return $('.dmhBulletEdit', context); }
	function $getBulletEditTemplate() { return $('#dmhBulletEditTemplate'); }
	function $getBulletEditText(context) { return $('.dmhBulletEditText', context); }
	function $getCancelHelpButton() { return $('#dmhCancelHelpButton'); }
	function $getCreateHelpButton() { return $('#dmhCreateHelpButton'); }
	function $getDialogTitle() { return $('.dmhDialogTitle'); }
	function $getDraggableBullet(context) { return $('>.dmhDraggableBullet', context); }
	function $getDraggableBulletNumber(context) { return $('.dmhDraggableBulletNumber', context); }
	function $getfirstTimeEdit() { return $('#dmhFirstTimeEdit'); }
	function $getfirstTimeEditBullets() { return $('#dmhFirstTimeEditBullets'); }
	function $getfirstTimeEditContent() { return $('#dmhFirstTimeEditContent'); }
	function $getfirstTimeEditMatchUrls() { return $('#dmhFirstTimeEditMatchUrls'); }
	function $getfirstTimeEditTitle() { return $('#dmhFirstTimeEditTitle'); }
	function $getSaveHelpButton() { return $('#dmhSaveHelpButton'); }


	// ** Private Methods **
	function AddBullet(bullet) {
		// Clone the template to create a new editor, initialize, and add to form.
		var $newBullet = $getBulletEditTemplate()
			.clone(true, true)
			.attr('id', 'dmhBulletEdit-' + bullet.Number);
		$newBullet.find('.dmhBulletNumber').html(bullet.Number);
		if (bullet.Text) {
			$getBulletEditText($newBullet).val(bullet.Text);
		}
		$getfirstTimeEditBullets().append($newBullet);

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
		$draggableBullet.attr('id', 'dmhDraggableBullet-' + bullet.Number)
			.css({ top: bulletTop + 'px', left: bulletLeft + 'px' })
			.draggable({
				containment: 'document',
				stop: DraggableBullet_DraggableStop,
			});
		UpdateBulletPositionValues($draggableBullet.get(0));
	}

	function CloseEditHelpForm() {
		$getDraggableBullet('body').remove();
		$getfirstTimeEdit().hide();
		_elements.$getModalMask().hide();
		$getCreateHelpButton().show();
	}

	function InitializeCreateMode() {
		$getSaveHelpButton().html('Create');
		$getDialogTitle().html('Create Help Page');

		$getfirstTimeEditTitle().val('Help - ' + document.title);
		$getfirstTimeEditMatchUrls().val(window.location.pathname);
		$getfirstTimeEditContent().val('');
		$getfirstTimeEditBullets().empty();

		var dialogLeft = ($(document).width() / 3 * 2) - ($getfirstTimeEdit().width());
		$getfirstTimeEdit().css({ top: '200px', left: dialogLeft });
	}

	function InitializeEditMode(page) {
		$getSaveHelpButton().html('Save');
		$getDialogTitle().html('Edit Help Page');

		$getfirstTimeEditTitle().val(page.Title);
		$getfirstTimeEditMatchUrls().val(page.MatchUrls);
		$getfirstTimeEditContent().val(page.Content);

		$getfirstTimeEditBullets().empty();
		$.each(page.Bullets, function (index, bullet) {
			AddBullet(bullet);
		});

		var anchor = $('#' + page.OffsetElementId);
		var top = (anchor.offset().top + page.VerticalOffset) + 'px';
		var left = (anchor.offset().left + page.HorizontalOffset) + 'px';
		$getfirstTimeEdit().css({ top: top, left: left });
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
		_currentPage.Title = $getfirstTimeEditTitle().val();
		_currentPage.MatchUrls = $getfirstTimeEditMatchUrls().val();
		_currentPage.SourceUrl = sourceUrl;
		_currentPage.Content = $getfirstTimeEditContent().val();

		$.each(_currentPage.Bullets, function (index, bullet) {
			bullet.Text = $getBulletEditText($('#dmhBulletEdit-' + (index + 1))).val();
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
		if (_currentPage)
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
		_elements.$getModalMask().show();

		var $form = $getfirstTimeEdit().show();
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
		var $editor = $(this).closest('.dmhBulletEdit');
		var number = $editor.find('.dmhBulletNumber').html();
		$editor.remove();
		$('#dmhDraggableBullet-' + number).remove();

		// Reassign all bullet numbers and element ids from top down.
		$getBulletEdit($getfirstTimeEditBullets()).each(function (index, editor) {
			var newNumber = index + 1;
			var $currentNumber = $(editor).find('.dmhBulletNumber');
			var currentNumber = $currentNumber.html();
			$currentNumber.html(newNumber);
			$(editor).attr('id', 'dmhBulletEdit-' + newNumber);
			var $draggableBullet = $('#dmhDraggableBullet-' + currentNumber).attr('id', 'dmhDraggableBullet-' + newNumber);
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
		var client = new DMH.AjaxClient();
		client.SaveFirstTimeHelp(_currentPage, function (page) {
			_currentPage.Id = page.Id;
		});

		$getCreateHelpButton().html('Edit Help Page');
		CloseEditHelpForm();

		$(_self).trigger('pageUpdate.dmh', _currentPage);
	}

	function Window_Resize() {
		_anchoredElementLogic.UpdateElementPositions(_currentPage, $getfirstTimeEdit());
	}


	// ** Public Methods ** 
	this.Initialize = function (applicationSettings, page) {
		_applicationSettings = applicationSettings;
		_currentPage = page;

		if (_applicationSettings && _applicationSettings.CanEdit) {

			// Initializes the form as much as possible.
			$getSaveHelpButton().click(SaveHelpButton_Click);
			$getCancelHelpButton().click(CancelHelpButton_Click);
			$getAddBulletButton().click(AddBulletButton_Click);
			$getBulletEditTemplate().find('.dmhButtonCancel').click(RemoveBulletButton_Click);
			$getfirstTimeEdit().draggable({
				handle: $getDialogTitle(),
				containment: 'document',
				stop: EditForm_DraggableStop,
			});
			// For bullet anchors, first find visible elements that have ids, 
			// contain no child nodes, are not help related and are not certain other tags.
			_leafElements = $('*[id]:visible:not(.dmh-,.dmh- *,link,meta,script,style,:has(*))');
			// Fall back on any non-help related elements with ids.
			if (_leafElements.length === 0) {
				_leafElements = $('*[id]:visible:not(.dmh-,.dmh- *,link,meta,script,style)');
			}
			// If there are still no elements, inform the user.
			if (_leafElements.length === 0) {
				$getCreateHelpButton()
					.html('Cannot Create Help')
					.attr('title', 'Click for more information.')
					.click(function () {
						alert("DocMAH was unable to find elements to anchor to. Please add IDs to elements you wish to place help near.");
					});
			} else {	// Otherwise, check to see if page should be in edit mode and attach edit event handler.
				if (_currentPage)
					$getCreateHelpButton().html('Edit Help Page');
				$getCreateHelpButton().click(CreateHelpButton_Click);
			}
		}

		$(window).resize(Window_Resize);
	}


	// ** Events ** 
	// pageUpdate.dmh
}

DMH.FirstTimeView = function () {
	// ** Private Fields **
	var _anchoredElementLogic = new DMH.AnchoredElementLogic();
	var _applicationSettings = null;
	var _currentPage = null;
	var _elements = new DMH.SharedElements();
	var _self = this;
	var _userPageSettings = null;
	var _nextBulletOffset = 0;		// Tracks left offset for unset bullets on documentation page.
	var $ = docmahJQuery;


	/** HTML Elements **/
	function $getBulletNumber(bulletElement) { return $('.dmhBulletNumber', bulletElement); }
	function $getBulletViewTemplate() { return $('#dmhBulletViewTemplate'); }
	function $getBulletViewText(bulletElement) { return $('.dmhBulletViewText', bulletElement); }
	function $getButtonBar() { return $('#dmhFirstTimeView .dmhButtonBar'); }
	function $getCloseHelpButton() { return $('#dmhCloseHelpButton'); }
	function $getDialogTitle() { return $('.dmhDialogTitle'); }
	function $getHideHelpButton() { return $('#dmhHideHelpButton'); }
	function $getPlacedBulletNumber(bulletElement) { return $('.dmhPlacedBulletNumber', bulletElement); }
	function $getPlacedBullets(context) { return $('>.dmhPlacedBullet', context); }
	function $getfirstTimeView() { return $('#dmhFirstTimeView'); }
	function $getfirstTimeViewBullets() { return $('#dmhFirstTimeViewBullets'); }
	function $getfirstTimeViewContent() { return $('#dmhFirstTimeViewContent'); }
	function $getShowDocumentationButton() { return $('#dmhShowDocumentationButton'); }


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

		var client = new DMH.AjaxClient();
		client.SaveUserPageSettings(_userPageSettings);
	}

	function PlaceBullet(bullet) {
		var $container = $('body');
		var anchorOffset = $('#' + bullet.OffsetElementId).offset();
		var top = anchorOffset ? anchorOffset.top + bullet.VerticalOffset : 0;
		var left = anchorOffset ? anchorOffset.left + bullet.HorizontalOffset : _nextBulletOffset;

		var $newBullet = $getBulletViewTemplate()
			.clone(true, true)
			.attr("id", 'dmhBulletView-' + bullet.Number);
		$getBulletNumber($newBullet).html(bullet.Number);
		$getBulletViewText($newBullet).html(bullet.Text);

		$getfirstTimeViewBullets().append($newBullet);

		var placedBullet = $getPlacedBullets($newBullet);
		$getPlacedBulletNumber(placedBullet).html(bullet.Number);
		placedBullet.detach();
		$container.append(placedBullet);
		placedBullet.attr('id', 'dmhPlacedBullet-' + bullet.Number)
			.css({ top: top, left: left });
	}

	function RemoveBullets() {
		$getPlacedBullets($('body')).remove();
	}


	// ** Event Handlers **
	function CloseHelpButtons_Click(e) {
		e.data.model.ClosePage();
		var cookies = new DMH.Cookies();
		cookies.SetCookie("DMH-HideHelp", e.data.hide.toString(), 730);
		AjaxSaveUserPageSettings(e.data.hide);
	}

	function Window_Resize() {
		_anchoredElementLogic.UpdateElementPositions(_currentPage, $getfirstTimeView());
		_elements.$getModalMask()
			.height($(document).height())
			.width($(document).width());
	}


	// ** Public Methods **
	this.ClosePage = function () {
		RemoveBullets();
		$getfirstTimeView().hide();
		$(document).off('resize');
		_elements.$getModalMask().hide();
	}

	this.Initialize = function (applicationSettings, firstTimeEdit, helpButton) {
		_applicationSettings = applicationSettings;

		$getCloseHelpButton().click({ model: this, hide: false }, CloseHelpButtons_Click);
		$getHideHelpButton().click({ model: this, hide: true }, CloseHelpButtons_Click);

		if (!_applicationSettings.DisableDocumentation) {
			$getShowDocumentationButton()
				.show()
				.click(function () { window.open(DMH.Urls.NavigateDocumentation, '_blank'); });
		} else {
			$getShowDocumentationButton().hide();
		}

		$(window).resize(Window_Resize);

		$(firstTimeEdit).bind("pageUpdate.dmh", function (event, newPage) {
			_currentPage = newPage;
		});
		$(helpButton).bind("showFirstTime.dmh", function () {
			_self.ShowPage();
		});
	}

	this.ShowPage = function (page) {
		// Use previous page if not present.
		if (page)
			_currentPage = page;

		if (_currentPage) {
			// This is needed on the documentation page when moving from one first time help to another first time help.
			// But it doesn't hurt to have for all other transitions.
			RemoveBullets();

			$getfirstTimeViewContent().html(_currentPage.Content);
			$getDialogTitle().html(_currentPage.Title);

			var view = $getfirstTimeView();
			_anchoredElementLogic.UpdateElementPosition(_currentPage, view);
			_elements.$getModalMask()
				.height($(document).height())
				.width($(document).width());

			_nextBulletOffset = 0;
			$getfirstTimeViewBullets().empty();
			$.each(_currentPage.Bullets, function (index, bullet) {
				PlaceBullet(bullet);
			});

			_elements.$getModalMask().show();
			view.show();
		}
	}

	this.ShowPageFirstTime = function (page, userPageSettings) {
		_userPageSettings = userPageSettings;
		_currentPage = page;

		var cookies = new DMH.Cookies();

		if (_currentPage &&
			cookies.GetCookie("DMH-HideHelp") !== "true" &&
			(_userPageSettings === null || !_userPageSettings.HidePage)) {
			this.ShowPage();
		}
	}
}

DMH.SharedElements = function () {
	var $ = docmahJQuery;

	/** HTML Elements **/
	this.$getModalMask = function () { return $('#dmhModalMask'); }
}
