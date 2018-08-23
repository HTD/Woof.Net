using System;
using System.Collections.Generic;
using Woof.Net.Http;
using Xunit;

namespace Woof.Net.Tests {

    public class OperationContractTests {

        /// <summary>
        /// Tests if parameters values can be passed to the operation contract in 7 different ways.
        /// </summary>
        [Fact]
        public void GetParameterValuesTest() {
            var binding = new ServiceBinding(typeof(TestService));
            var s1 = binding.OperationContracts[0].Signature;
            var s2 = binding.OperationContracts[1].Signature;
            var s3 = binding.OperationContracts[2].Signature;
            var s4 = binding.OperationContracts[3].Signature;
            var s5 = binding.OperationContracts[4].Signature;
            var s6 = binding.OperationContracts[5].Signature;
            var s7 = binding.OperationContracts[6].Signature;
            var s2pattern = binding.OperationContracts[1].Metadata.UriPattern;
            var qs = "b=true&i=-1&f=0.1&d=0.2&m=0.3&dt=2018-08-17T06:30:29.542Z&s=test";
            var rl = "tm2/true/-1/0.1/0.2/0.3/2018-08-17T06:30:29.542Z/test";
            var v1 = OperationContract.GetParameterValues(s1, binding.ContractSerializer, qs);
            var v2 = OperationContract.GetParameterValues(s2, binding.ContractSerializer, rl, s2pattern);
            var v3 = OperationContract.GetParameterValues(s3, binding.ContractSerializer, qs);
            var v4 = OperationContract.GetParameterValues(s4, binding.ContractSerializer, qs);
            var v5 = OperationContract.GetParameterValues(s5, binding.ContractSerializer, qs);
            var v6 = OperationContract.GetParameterValues(s6, binding.ContractSerializer, qs);
            var v7 = OperationContract.GetParameterValues(s7, binding.ContractSerializer, qs);

            Assert.Equal(7, v1.Length);
            Assert.Equal(7, v2.Length);

            Assert.Equal(typeof(bool), v1[0].GetType());
            Assert.Equal(typeof(int), v1[1].GetType());
            Assert.Equal(typeof(float), v1[2].GetType());
            Assert.Equal(typeof(double), v1[3].GetType());
            Assert.Equal(typeof(decimal), v1[4].GetType());
            Assert.Equal(typeof(DateTime), v1[5].GetType());
            Assert.Equal(typeof(string), v1[6].GetType());

            Assert.Equal(typeof(bool), v2[0].GetType());
            Assert.Equal(typeof(int), v2[1].GetType());
            Assert.Equal(typeof(float), v2[2].GetType());
            Assert.Equal(typeof(double), v2[3].GetType());
            Assert.Equal(typeof(decimal), v2[4].GetType());
            Assert.Equal(typeof(DateTime), v2[5].GetType());
            Assert.Equal(typeof(string), v2[6].GetType());

            Assert.Equal(typeof(bool), (v3[0] as Type1).B.GetType());
            Assert.Equal(typeof(int), (v3[0] as Type1).I.GetType());
            Assert.Equal(typeof(float), (v3[0] as Type1).F.GetType());
            Assert.Equal(typeof(double), (v3[0] as Type1).D.GetType());
            Assert.Equal(typeof(decimal), (v3[0] as Type1).M.GetType());
            Assert.Equal(typeof(DateTime), (v3[0] as Type1).DT.GetType());
            Assert.Equal(typeof(string), (v3[0] as Type1).S.GetType());

            Assert.Equal(typeof(bool), (v4[0] as Type2).B.GetType());
            Assert.Equal(typeof(int), (v4[0] as Type2).I.GetType());
            Assert.Equal(typeof(float), (v4[0] as Type2).F.GetType());
            Assert.Equal(typeof(double), (v4[0] as Type2).D.GetType());
            Assert.Equal(typeof(decimal), (v4[0] as Type2).M.GetType());
            Assert.Equal(typeof(DateTime), (v4[0] as Type2).DT.GetType());
            Assert.Equal(typeof(string), (v4[0] as Type2).S.GetType());

            Assert.Equal(typeof(bool), (v5[0] as Type3).B.GetType());
            Assert.Equal(typeof(int), (v5[0] as Type3).I.GetType());
            Assert.Equal(typeof(float), (v5[0] as Type3).F.GetType());
            Assert.Equal(typeof(double), (v5[0] as Type3).D.GetType());
            Assert.Equal(typeof(decimal), (v5[0] as Type3).M.GetType());
            Assert.Equal(typeof(DateTime), (v5[0] as Type3).DT.GetType());
            Assert.Equal(typeof(string), (v5[0] as Type3).S.GetType());

            Assert.Equal(typeof(bool), ((Type4)v6[0]).B.GetType());
            Assert.Equal(typeof(int), ((Type4)v6[0]).I.GetType());
            Assert.Equal(typeof(float), ((Type4)v6[0]).F.GetType());
            Assert.Equal(typeof(double), ((Type4)v6[0]).D.GetType());
            Assert.Equal(typeof(decimal), ((Type4)v6[0]).M.GetType());
            Assert.Equal(typeof(DateTime), ((Type4)v6[0]).DT.GetType());
            Assert.Equal(typeof(string), ((Type4)v6[0]).S.GetType());

            Assert.Equal(typeof(bool), ((IDictionary<string, object>)v7[0])["b"].GetType());
            Assert.Equal(typeof(long), ((IDictionary<string, object>)v7[0])["i"].GetType());
            Assert.Equal(typeof(double), ((IDictionary<string, object>)v7[0])["f"].GetType());
            Assert.Equal(typeof(double), ((IDictionary<string, object>)v7[0])["d"].GetType());
            Assert.Equal(typeof(double), ((IDictionary<string, object>)v7[0])["m"].GetType());
            Assert.Equal(typeof(DateTime), ((IDictionary<string, object>)v7[0])["dt"].GetType());
            Assert.Equal(typeof(string), ((IDictionary<string, object>)v7[0])["s"].GetType());

        }

    }

    #region Test assets

    [ServiceContract]
    class TestService {

        [OperationContract]
        public string TestMethod1(bool b, int i, float f, double d, decimal m, DateTime dt, string s) => "OK";
        

        [OperationContract("GET", "tm2/{b}/{i}/{f}/{d}/{m}/{dt}/{s}")]
        public string TestMethod2(bool b, int i, float f, double d, decimal m, DateTime dt, string s) => "OK";

        [OperationContract]
        public string TestMethod3(Type1 x) => "OK";
        

        [OperationContract]
        public string TestMethod4(Type2 x) => "OK";

        [OperationContract]
        public string TestMethod5(Type3 x) => "OK";

        [OperationContract]
        public string TestMethod6(Type4 x) => "OK";

        [OperationContract]
        public string TestMethod7(object x) => "OK";

    }

    #region Test types

#pragma warning disable 0649

    class Type1 {

        public bool B;
        public int I;
        public float F;
        public double D;
        public decimal M;
        public DateTime DT;
        public string S;

    }

    class Type2 {

        public bool B { get; set; }
        public int I { get; set; }
        public float F { get; set; }
        public double D { get; set; }
        public decimal M { get; set; }
        public DateTime DT { get; set; }
        public string S { get; set; }

    }

    abstract class Type3Base {

        public bool B { get; set; }
        public int I { get; set; }
        public float F { get; set; }
        public double D { get; set; }
        public decimal M { get; set; }
        public DateTime DT { get; set; }
        public string S { get; set; }

    }

    class Type3 : Type3Base { }

    struct Type4 {

        public bool B;
        public int I;
        public float F;
        public double D;
        public decimal M;
        public DateTime DT;
        public string S;

    }

#pragma warning restore 0649

    #endregion

    #endregion

}