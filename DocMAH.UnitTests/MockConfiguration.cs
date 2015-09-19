using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocMAH.Configuration;
using Moq;

namespace DocMAH.UnitTests
{
	public class MockConfiguration
	{
		#region Constructors

		public MockConfiguration(MockRepository mocks)
		{
			_mocks = mocks;
		}

		#endregion

		#region Private Fields

		private readonly MockRepository _mocks;

		private Mock<IClosePopupButtonConfiguration> _closePopupButtonConfiguration;
		private Mock<IDocmahConfiguration> _docmahConfiguration;
		private Mock<IDocumentationConfiguration> _documentationConfiguration;
		private Mock<IEditHelpConfiguration> _editHelpConfiguration;
		private Mock<IHidePopupButtonConfiguration> _hidePopupButtonConfiguration;
		private Mock<IPopupViewerConfiguration> _popupViewerConfiguration;

		#endregion

		#region Public Properties

		public Mock<IClosePopupButtonConfiguration> ClosePopupButtonConfiguration
		{
			get
			{
				if (null == _closePopupButtonConfiguration)
				{
					_closePopupButtonConfiguration = _mocks.Create<IClosePopupButtonConfiguration>();
					PopupViewerConfiguration.Setup(c => c.ClosePopupButtonConfiguration).Returns(_closePopupButtonConfiguration.Object);
				}
				return _closePopupButtonConfiguration;
			}
		}

		public Mock<IDocmahConfiguration> DocmahConfiguration
		{
			get
			{
				if (null == _docmahConfiguration)
				{
					_docmahConfiguration = _mocks.Create<IDocmahConfiguration>();
				}
				return _docmahConfiguration;
			}
		}

		public Mock<IDocumentationConfiguration> DocumentationConfiguration
		{
			get
			{
				if (null == _documentationConfiguration)
				{
					_documentationConfiguration = _mocks.Create<IDocumentationConfiguration>();
					DocmahConfiguration.Setup(c => c.DocumentationConfiguration).Returns(_documentationConfiguration.Object);
				}
				return _documentationConfiguration;
			}
		}

		public Mock<IEditHelpConfiguration> EditHelpConfiguration
		{
			get
			{
				if (null == _editHelpConfiguration)
				{
					_editHelpConfiguration = _mocks.Create<IEditHelpConfiguration>();
					DocmahConfiguration.Setup(c => c.EditHelpConfiguration).Returns(_editHelpConfiguration.Object);
				}
				return _editHelpConfiguration;
			}
		}

		public Mock<IHidePopupButtonConfiguration> HidePopupButtonElement
		{
			get
			{
				if (null == _hidePopupButtonConfiguration)
				{
					_hidePopupButtonConfiguration = _mocks.Create<IHidePopupButtonConfiguration>();
					PopupViewerConfiguration.Setup(c => c.HidePopupButtonConfiguration).Returns(_hidePopupButtonConfiguration.Object);
				}
				return _hidePopupButtonConfiguration;
			}
		}

		/// <summary>
		/// Convenience reference to root configuration object
		/// </summary>
		public IDocmahConfiguration Object
		{
			get { return DocmahConfiguration.Object; }
		}

		public Mock<IPopupViewerConfiguration> PopupViewerConfiguration
		{
			get
			{
				if (null == _popupViewerConfiguration)
				{
					_popupViewerConfiguration = _mocks.Create<IPopupViewerConfiguration>();
					DocmahConfiguration.Setup(c => c.PopupViewerConfiguration).Returns(_popupViewerConfiguration.Object);
				}
				return _popupViewerConfiguration;
			}
		}
		#endregion
	}
}
