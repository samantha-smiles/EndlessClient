// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Data.Account
{
	public class ChangePasswordParameters : IChangePasswordParameters
	{
		public string AccountName { get; private set; }
		public string OldPassword { get; private set; }
		public string NewPassword { get; private set; }
		public string ConfirmNewPassword { get; private set; }

		public ChangePasswordParameters(string accountName,
			string oldPassword,
			string newPassword,
			string confirmNewPassword)
		{
			AccountName = accountName;
			OldPassword = oldPassword;
			NewPassword = newPassword;
			ConfirmNewPassword = confirmNewPassword;
		}
	}
}