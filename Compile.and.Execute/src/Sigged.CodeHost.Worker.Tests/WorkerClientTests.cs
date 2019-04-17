using Sigged.CodeHost.Core.Dto;
using Sigged.CodeHost.Core.Worker;
using Sigged.CodeHost.Worker.Tests.Mock;
using Sigged.CodeHost.Worker.Tests.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Xunit;

namespace Sigged.CodeHost.Worker.Tests
{
    public class WorkerClientTests
    {
        WorkerClientArrangement arrangement = null;

        public WorkerClientTests()
        {
            arrangement = new WorkerClientArrangement();
        }
        
        [Fact]
        public async void WorkerClient_Connects_To_WorkerService()
        {
            //arrange
            bool actualConnected = false; //hold actual result

            //act
            arrangement.WorkerService.WorkerConnected += delegate (TcpClient workerClient, string sessionId) {
                actualConnected = workerClient.Connected;

                arrangement.Worker.Stop();
                arrangement.WorkerService.StopListening();
            };

            arrangement.WorkerService.StartListening();
            await arrangement.Worker.Start(arrangement.ServiceHostName, arrangement.ServicePort, arrangement.SessionId);

            //assert
            Assert.True(actualConnected);
        }

        [Fact]
        public async void WorkerClient_Builds_On_Request()
        {
            //arrange
            BuildResultDto actualResult = null; //hold actual result

            var buildRequest = new BuildRequestDto
            {
                SessionId = arrangement.SessionId,
                RunOnSuccess = false,
                SourceCode = MockSourceCodeRepository.Get_Working_SimpleOutput_Code()
            };

            //act
            arrangement.WorkerService.StartListening();

            arrangement.WorkerService.WorkerConnected += (TcpClient workerClient, string sessionId) => {
                arrangement.WorkerService.SendWorkerMessage(workerClient, MessageType.ServerBuildRequest, buildRequest);
            };

            arrangement.WorkerService.WorkerCompletedBuild += delegate (TcpClient workerClient, BuildResultDto message) {
                arrangement.Worker?.Stop();
                arrangement.WorkerService?.StopListening();

                actualResult = message;
            };

            await arrangement.Worker.Start(arrangement.ServiceHostName, arrangement.ServicePort, arrangement.SessionId);

            //assert
            Assert.True(actualResult.IsSuccess);
        }

        [Fact]
        public async void WorkerClient_Reports_ExecutionState()
        {
            //arrange
            Queue<ExecutionStateDto> actualStates = new Queue<ExecutionStateDto>(); //hold actual result

            var buildRequest = new BuildRequestDto
            {
                SessionId = arrangement.SessionId,
                RunOnSuccess = true,
                SourceCode = MockSourceCodeRepository.Get_Working_SimpleOutput_Code()
            };

            //act
            arrangement.WorkerService.StartListening();
            arrangement.WorkerService.WorkerConnected += (TcpClient workerClient, string sessionId) => {
                arrangement.WorkerService.SendWorkerMessage(workerClient, MessageType.ServerBuildRequest, buildRequest);
            };

            arrangement.WorkerService.WorkerExecutionStateChanged += delegate (TcpClient workerClient, ExecutionStateDto message) {
                arrangement.Worker?.Stop();
                arrangement.WorkerService?.StopListening();

                actualStates.Enqueue(message);
            };

            await arrangement.Worker.Start(arrangement.ServiceHostName, arrangement.ServicePort, arrangement.SessionId);
            
            //assert 
            ExecutionStateDto nextState;

            nextState = actualStates.Dequeue();
            Assert.Equal(RemoteAppState.Running, nextState.State);

            nextState = actualStates.Dequeue();
            Assert.Equal(RemoteAppState.WriteOutput, nextState.State);
            Assert.Equal("All your base are belong to us.", nextState.Output);

            nextState = actualStates.Dequeue();
            Assert.Equal(RemoteAppState.WriteOutput, nextState.State);
            Assert.Equal("\r\n", nextState.Output);

            nextState = actualStates.Dequeue();
            Assert.Equal(RemoteAppState.Ended, nextState.State);
        }
        
        [Theory]
        [MemberData(nameof(MockSourceCodeRepository.Get_Bad_MainMethod_Codes), MemberType = typeof(MockSourceCodeRepository))]
        public async void WorkerClient_Build_Fails_When_Bad_EntryPoint(string source, string expectedErrorCode)
        {
            //arrange
            BuildResultDto actualResult = null; //hold actual result

            var buildRequest = new BuildRequestDto
            {
                SessionId = arrangement.SessionId,
                RunOnSuccess = true,
                SourceCode = source
            };

            //act
            arrangement.WorkerService.StartListening();
            arrangement.WorkerService.WorkerConnected += (TcpClient workerClient, string sessionId) => {
                arrangement.WorkerService.SendWorkerMessage(workerClient, MessageType.ServerBuildRequest, buildRequest);
            };

            arrangement.WorkerService.WorkerCompletedBuild += delegate (TcpClient workerClient, BuildResultDto message) {
                arrangement.Worker?.Stop();
                arrangement.WorkerService?.StopListening();

                actualResult = message;
            };

            await arrangement.Worker.Start(arrangement.ServiceHostName, arrangement.ServicePort, arrangement.SessionId);


            //assert 
            Assert.False(actualResult.IsSuccess);
            
            Assert.Equal(expectedErrorCode, actualResult.BuildErrors.First().Id); 
        }


        [Fact]
        public async void WorkerClient_Build_Fails_When_Ambiguous_EntryPoint()
        {
            //arrange
            BuildResultDto actualResult = null; //hold actual result

            var buildRequest = new BuildRequestDto
            {
                SessionId = arrangement.SessionId,
                RunOnSuccess = true,
                SourceCode = MockSourceCodeRepository.Get_AmbiguousMain_Code()
            };

            //act
            arrangement.WorkerService.StartListening();
            arrangement.WorkerService.WorkerConnected += (TcpClient workerClient, string sessionId) => {
                arrangement.WorkerService.SendWorkerMessage(workerClient, MessageType.ServerBuildRequest, buildRequest);
            };

            arrangement.WorkerService.WorkerCompletedBuild += delegate (TcpClient workerClient, BuildResultDto message) {
                arrangement.Worker?.Stop();
                arrangement.WorkerService?.StopListening();

                actualResult = message;
            };

            await arrangement.Worker.Start(arrangement.ServiceHostName, arrangement.ServicePort, arrangement.SessionId);

            //assert 
            Assert.False(actualResult.IsSuccess);

            //CS0017 = Program has more than one entry point defined. Compile with /main to specify the type that contains the entry point.
            Assert.Equal("CS0017", actualResult.BuildErrors.First().Id);
        }


        [Theory]
        [MemberData(nameof(MockSourceCodeRepository.Get_Varying_MainParms_Codes), MemberType = typeof(MockSourceCodeRepository))]
        public async void WorkerClient_Calls_EntryPoint_With_Correct_Arguments(string source)
        {
            //arrange
            //var arrangement = new WorkerClientArrangement(); //rearrange per theory run (ports!)
            Queue<ExecutionStateDto> actualStates = new Queue<ExecutionStateDto>(); //hold actual result

            var buildRequest = new BuildRequestDto
            {
                SessionId = arrangement.SessionId,
                RunOnSuccess = true,
                SourceCode = source,
            };

            //act
            arrangement.WorkerService.StartListening();
            arrangement.WorkerService.WorkerConnected += (TcpClient workerClient, string sessionId) => {
                arrangement.WorkerService.SendWorkerMessage(workerClient, MessageType.ServerBuildRequest, buildRequest);
            };

            arrangement.WorkerService.WorkerExecutionStateChanged += delegate (TcpClient workerClient, ExecutionStateDto message) {
                actualStates.Enqueue(message);
                if (actualStates.Count >= 2) //if crashed, this will be in the second state
                {
                    arrangement.Worker?.Stop();
                    arrangement.WorkerService?.StopListening();
                }
            };

            await arrangement.Worker.Start(arrangement.ServiceHostName, arrangement.ServicePort, arrangement.SessionId);
            
            //assert 
            Assert.Empty(actualStates.Where(s => s.Exception != null));
            Assert.Empty(actualStates.Where(s => s.State == RemoteAppState.Crashed));
        }
    }
}
