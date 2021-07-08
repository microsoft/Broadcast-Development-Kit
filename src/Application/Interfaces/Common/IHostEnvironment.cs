// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
namespace Application.Interfaces.Common
{
    public interface IHostEnvironment
    {
        string EnvironmentName { get; }

        bool IsDevelopment();

        bool IsProduction();

        bool IsLocal();
    }
}
