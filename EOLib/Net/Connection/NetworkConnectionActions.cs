﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Threading.Tasks;
using EOLib.Data.Protocol;
using EOLib.IO.Repositories;
using EOLib.IO.Services;
using EOLib.Net.Communication;
using EOLib.Net.PacketProcessing;

namespace EOLib.Net.Connection
{
	public class NetworkConnectionActions : INetworkConnectionActions
	{
		private readonly INetworkClientProvider _networkClientProvider;
		private readonly IConfigurationProvider _configurationProvider;
		private readonly IHashService _hashService;
		private readonly IHDSerialNumberService _hdSerialNumberService;
		private readonly IInitDataGeneratorService _initDataGeneratorService;

		public NetworkConnectionActions(INetworkClientProvider networkClientProvider,
										IConfigurationProvider configurationProvider,
										IHashService hashService,
										IHDSerialNumberService hdSerialNumberService,
										IInitDataGeneratorService initDataGeneratorService)
		{
			_networkClientProvider = networkClientProvider;
			_configurationProvider = configurationProvider;
			_hashService = hashService;
			_hdSerialNumberService = hdSerialNumberService;
			_initDataGeneratorService = initDataGeneratorService;
		}

		public async Task<ConnectResult> ConnectToServer()
		{
			if (Client.Connected)
				return ConnectResult.Success;

			var host = _configurationProvider.Host;
			var port = _configurationProvider.Port;

			return await Client.ConnectToServer(host, port);
		}

		public async Task<IInitializationData> BeginHandshake()
		{
			var stupidHash = _hashService.StupidHash(new Random().Next(6, 12));
			var hdSerialNumber = _hdSerialNumberService.GetHDSerialNumber();

			var packet = new PacketBuilder(PacketFamily.Init, PacketAction.Init)
				.AddThree(stupidHash)
				.AddChar(_configurationProvider.VersionMajor)
				.AddChar(_configurationProvider.VersionMinor)
				.AddChar(_configurationProvider.VersionBuild)
				.AddChar(112)
				.AddChar((byte)hdSerialNumber.Length)
				.AddString(hdSerialNumber)
				.Build();

			var bytes = Client.Send(packet);
			if (bytes == 0)
				throw new NoDataSentException();

			var responsePacket = await Client.PacketQueue.WaitForPacketAndDequeue();
			if (IsInvalidInitPacket(responsePacket))
				throw new EmptyPacketReceivedException();

			return _initDataGeneratorService.GetInitData(responsePacket);
		}

		public void CompleteHandshake()
		{
			throw new NotImplementedException();
		}

		private static bool IsInvalidInitPacket(IPacket responsePacket)
		{
			return responsePacket is EmptyPacket || (responsePacket.Family != PacketFamily.Init && responsePacket.Action != PacketAction.Init);
		}

		private INetworkClient<IPacketQueue> Client { get { return _networkClientProvider.NetworkClient; } }
	}
}