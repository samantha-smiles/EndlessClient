﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EndlessClient.GameExecution;
using EndlessClient.UIControls;
using EOLib;
using EOLib.Graphics;
using EOLib.IO.Repositories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.ControlSets
{
	public class InitialControlSet : BaseControlSet
	{
		private readonly IConfigurationProvider _configProvider;

		private IGameComponent _createAccount,
							   _login,
							   _viewCredits,
							   _exitGame,
							   _versionInfo,
							   _personPicture;

		private readonly Texture2D[] _personSet1;
		private readonly Random _randomGen;

		public override GameStates GameState { get { return GameStates.Initial; } }

		public InitialControlSet(IConfigurationProvider configProvider)
		{
			_configProvider = configProvider;
			_personSet1 = new Texture2D[4];
			_randomGen = new Random();
		}

		public override void InitializeResources(INativeGraphicsManager gfxManager, ContentManager xnaContentManager)
		{
			base.InitializeResources(gfxManager, xnaContentManager);

			for (int i = 0; i < _personSet1.Length; ++i)
				_personSet1[i] = gfxManager.TextureFromResource(GFXTypes.PreLoginUI, 41 + i, true);
		}

		protected override void InitializeControlsHelper(IControlSet currentControlSet)
		{
			_createAccount = GetControl(currentControlSet, GameControlIdentifier.InitialCreateAccount, GetMainCreateAccountButton);
			_login = GetControl(currentControlSet, GameControlIdentifier.InitialLogin, GetMainLoginButton);
			_viewCredits = GetControl(currentControlSet, GameControlIdentifier.InitialViewCredits, GetViewCreditsButton);
			_exitGame = GetControl(currentControlSet, GameControlIdentifier.InitialExitGame, GetExitButton);
			_versionInfo = GetControl(currentControlSet, GameControlIdentifier.InitialVersionLabel, GetVersionInfoLabel);
			_personPicture = GetControl(currentControlSet, GameControlIdentifier.PersonDisplay1, GetPersonPicture1);

			_allComponents.Add(_createAccount);
			_allComponents.Add(_login);
			_allComponents.Add(_viewCredits);
			_allComponents.Add(_exitGame);
			_allComponents.Add(_versionInfo);
			_allComponents.Add(_personPicture);
		}

		public override IGameComponent FindComponentByControlIdentifier(GameControlIdentifier control)
		{
			switch (control)
			{
				case GameControlIdentifier.InitialCreateAccount: return _createAccount;
				case GameControlIdentifier.InitialLogin: return _login;
				case GameControlIdentifier.InitialViewCredits: return _viewCredits;
				case GameControlIdentifier.InitialExitGame: return _exitGame;
				case GameControlIdentifier.InitialVersionLabel: return _versionInfo;
				case GameControlIdentifier.PersonDisplay1: return _personPicture;
				default: return null;
			}
		}

		private XNAButton GetMainCreateAccountButton()
		{
			return MainButtonCreationHelper(GameControlIdentifier.InitialCreateAccount);
		}

		private XNAButton GetMainLoginButton()
		{
			return MainButtonCreationHelper(GameControlIdentifier.InitialLogin);
		}

		private XNAButton GetViewCreditsButton()
		{
			return MainButtonCreationHelper(GameControlIdentifier.InitialViewCredits);
		}

		private XNAButton GetExitButton()
		{
			return MainButtonCreationHelper(GameControlIdentifier.InitialExitGame);
		}

		private XNAButton MainButtonCreationHelper(GameControlIdentifier whichControl)
		{
			int i;
			switch (whichControl)
			{
				case GameControlIdentifier.InitialCreateAccount: i = 0; break;
				case GameControlIdentifier.InitialLogin: i = 1; break;
				case GameControlIdentifier.InitialViewCredits: i = 2; break;
				case GameControlIdentifier.InitialExitGame: i = 3; break;
				default: throw new ArgumentException("Invalid control specified for helper", "whichControl");
			}

			var widthFactor = _mainButtonTexture.Width / 2;
			var heightFactor = _mainButtonTexture.Height / 4;
			var outSource = new Rectangle(0, i * heightFactor, widthFactor, heightFactor);
			var overSource = new Rectangle(widthFactor, i * heightFactor, widthFactor, heightFactor);

			return new XNAButton(_mainButtonTexture, new Vector2(26, 278 + i * 40), outSource, overSource);
		}

		private XNALabel GetVersionInfoLabel()
		{
			return new XNALabel(new Rectangle(25, 453, 1, 1), Constants.FontSize07)
			{
				Text = string.Format(Constants.VersionInfoFormat,
									 _configProvider.VersionMajor,
									 _configProvider.VersionMinor,
									 _configProvider.VersionBuild,
									 _configProvider.Host,
									 _configProvider.Port),
				ForeColor = Constants.BeigeText
			};
		}

		private PictureBox GetPersonPicture1()
		{
			var texture = _personSet1[_randomGen.Next(4)];
			return new PictureBox(texture) { DrawLocation = new Vector2(229, 70) };
		}

		protected void ExcludePersonPicture1()
		{
			((PictureBox) _personPicture).Close();
			_allComponents.Remove(_personPicture);
		}
	}
}