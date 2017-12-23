﻿namespace SmtpServerStub.Enums
{
    internal enum ResponseCodes
	{
		NonStdSuccess,
		SysHelp,
		HelpMsg,
		SrvReady,
		SrvClosingChannel,
        SrvHello,
		RqstActOkCompleted,
		UsrWillForward,
		CantVrfyUserAttemptDelivery,
		StrtInputEndWith,
		SrvNotAvailableClose,
		RqstMailActNotTakenMbUnavailable,
		RqstActAbortErProcessing,
		RqstActNotTakenSysStorage,
		SyntaxErrorCommand,
		SyntaxErrorParam,
		CommandNotImplemented,
		BadCommandsSequence,
		CommandParamNotImplemented,
		DoesNotAcceptMail,
		AccessDenied,
		RqstActNotTakenMbUnavailable,
		TryFwd,
		StorageAllocationLimit,
		MbNameNotAllowed,
		TransactionFailed
	}
}