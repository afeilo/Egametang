﻿using System;
using Model;
using UnityEngine;
using UnityEngine.UI;

namespace Hotfix
{
	[ObjectEvent]
	public class UILoginComponentEvent : ObjectEvent<UILoginComponent>, IAwake
	{
		public void Awake()
		{
			this.Get().Awake();
		}
	}
	
	public class UILoginComponent: Component
	{
		private GameObject account;
		private GameObject loginBtn;

		public void Awake()
		{
			ReferenceCollector rc = this.GetEntity<UI>().GameObject.GetComponent<ReferenceCollector>();
			loginBtn = rc.Get<GameObject>("LoginBtn");
			loginBtn.GetComponent<Button>().onClick.Add(OnLogin);

			this.account = rc.Get<GameObject>("Account");
		}

		private void OnLogin()
		{
			Session session = null;
			session = Game.Scene.GetComponent<NetOuterComponent>().Create(GlobalConfigComponent.Instance.GlobalProto.Address);
			string text = this.account.GetComponent<InputField>().text;
			session.CallWithAction(new C2R_Login() { Account = text, Password = "111111" }, (response) => LoginOK(response));
		}

		private void LoginOK(AResponse response)
		{
			R2C_Login r2CLogin = (R2C_Login) response;
			if (r2CLogin.Error != ErrorCode.ERR_Success)
			{
				Log.Error(r2CLogin.Error.ToString());
				return;
			}

			Session gateSession = Game.Scene.GetComponent<NetOuterComponent>().Create(r2CLogin.Address);
			Game.Scene.AddComponent<SessionComponent>().Session = gateSession;

			SessionComponent.Instance.Session.CallWithAction(new C2G_LoginGate() { Key = r2CLogin.Key },
				(response2)=>LoginGateOk(response2)
			);

		}

		private void LoginGateOk(AResponse response)
		{
			G2C_LoginGate g2CLoginGate = (G2C_LoginGate) response;
			if (g2CLoginGate.Error != ErrorCode.ERR_Success)
			{
				Log.Error(g2CLoginGate.Error.ToString());
				return;
			}

			Log.Info("登陆gate成功!");

			// 创建Player
			Player player = Model.EntityFactory.CreateWithId<Player>(g2CLoginGate.PlayerId);
			PlayerComponent playerComponent = Game.Scene.GetComponent<PlayerComponent>();
			playerComponent.MyPlayer = player;

			Hotfix.Scene.GetComponent<UIComponent>().Create(UIType.UILobby);
			Hotfix.Scene.GetComponent<UIComponent>().Remove(UIType.UILogin);
		}
	}
}
