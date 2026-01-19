using Integration.Tests.Client;
using Shared.Model;

namespace Integration.Tests.Base
{
    public class TestPacketResult<T> where T : ResponseBase
    {
        public TestUserContext Uc;
        public T Body;
        public int ResultCode;
    }

    public class TestNtfPacketResult<T> where T : NtfBase
    {
        public TestUserContext Uc;
        public T Body;
        public int ResultCode;
    }

    public class MultipleTestPacketResult<T, T2>
        where T : ResponseBase where T2 : NtfBase
    {
        public TestPacketResult<T> Main;
        public TestNtfPacketResult<T2>[] Ntf;
    }

    public class MultipleTestPacketResult<T, T2, T3>
        where T : ResponseBase
        where T2 : NtfBase
        where T3 : NtfBase
    {
        public TestPacketResult<T> Main;
        public TestNtfPacketResult<T2>[] Ntf0;
        public TestNtfPacketResult<T3>[] Ntf1;
    }

    public class MultipleTestPacketResult<T, T2, T3, T4>
        where T : ResponseBase
        where T2 : NtfBase
        where T3 : NtfBase
        where T4 : NtfBase
    {
        public TestPacketResult<T> Main;
        public TestNtfPacketResult<T2>[] Ntf0;
        public TestNtfPacketResult<T3>[] Ntf1;
        public TestNtfPacketResult<T4>[] Ntf2;
    }

    public class MultipleTestPacketResult<T, T2, T3, T4, T5>
        where T : ResponseBase
        where T2 : NtfBase
        where T3 : NtfBase
        where T4 : NtfBase
        where T5 : NtfBase
    {
        public TestPacketResult<T> Main;
        public TestNtfPacketResult<T2>[] Ntf0;
        public TestNtfPacketResult<T3>[] Ntf1;
        public TestNtfPacketResult<T4>[] Ntf2;
        public TestNtfPacketResult<T5>[] Ntf3;
    }

    public class MultipleTestPacketResult<T, T2, T3, T4, T5, T6>
        where T : ResponseBase
        where T2 : NtfBase
        where T3 : NtfBase
        where T4 : NtfBase
        where T5 : NtfBase
        where T6 : NtfBase
    {
        public TestPacketResult<T> Main;
        public TestNtfPacketResult<T2>[] Ntf0;
        public TestNtfPacketResult<T3>[] Ntf1;
        public TestNtfPacketResult<T4>[] Ntf2;
        public TestNtfPacketResult<T5>[] Ntf3;
        public TestNtfPacketResult<T6>[] Ntf4;
    }


    public class GroupTestPacketResult<T, T2>
         where T : ResponseBase 
        where T2 : NtfBase
    {
        public TestPacketResult<T>[] Main;
        public TestNtfPacketResult<T2>[] Ntf;
    }
}
