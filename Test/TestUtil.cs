using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Microsoft.AspNetCore.TestHost;
using Protacon.NetCore.WebApi.TestUtil;

namespace Pdf.Storage.Test
{
    public static class TestUtil
    {
        public static CallResponse WaitForOk(this TestServer host, string path, string reason = "Timeout")
        {
            var errors = new List<Exception>();
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    var response = host.Get(path).ExpectStatusCode(HttpStatusCode.OK);
                    return response;
                }
                catch (ExpectedStatusCodeException ex)
                {
                    errors.Add(ex);
                    Thread.Sleep(1000);
                }
            }

            throw new AggregateException(errors);
        }
    }
}
