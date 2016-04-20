﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Data.AccountCreation
{
	public interface ICreateAccountActions
	{
		CreateAccountParameterResult CheckAccountCreateParameters(ICreateAccountParameters createAccountCreateparameters);
	}
}