using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Protacon.NetCore.WebApi.TestUtil;

namespace Pdf.Storage.Test
{
    public static class TestUtil
    {
        public static CallResponse WaitForOk(this TestHost host, string path, string reason = "Timeout")
        {
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    var response = host.Get(path).ExpectStatusCode(HttpStatusCode.OK);
                    return response;
                }
                catch (ExpectedStatusCodeException)
                {
                    Thread.Sleep(1000);
                }
            }

            throw new InvalidOperationException(reason);
        }
    }
}
