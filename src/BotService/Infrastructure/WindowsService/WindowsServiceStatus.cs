// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Runtime.InteropServices;

#pragma warning disable SA1307 // We cannot change the name of the struct fields to match our naming conventions
namespace BotService.Infrastructure.Extensions
{
    [StructLayout(LayoutKind.Sequential)]
    public struct WindowsServiceStatus
    {
        public int dwServiceType;
        public WindowsServiceState dwCurrentState;
        public int dwControlsAccepted;
        public int dwWin32ExitCode;
        public int dwServiceSpecificExitCode;
        public int dwCheckPoint;
        public int dwWaitHint;
    }
}
#pragma warning restore SA1307