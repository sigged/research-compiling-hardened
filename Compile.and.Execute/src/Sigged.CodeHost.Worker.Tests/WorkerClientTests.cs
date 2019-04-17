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
        
        [Fact]
        public async void WorkerClient_Build_Fails_When_No_EntryPoint()
        {
            //arrange
            BuildResultDto actualResult = null; //hold actual result

            var buildRequest = new BuildRequestDto
            {
                SessionId = arrangement.SessionId,
                RunOnSuccess = true,
                SourceCode = MockSourceCodeRepository.Get_NoMainMethod_Code()
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
            
            //CS5001 = Program 'program' does not contain a static 'Main' method suitable for an entry point
            Assert.Equal("CS5001", actualResult.BuildErrors.First().Id); 
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


        [Fact]
        public async void WorkerClient_Calls_EntryPoint_With_Correct_Arguments()
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
    }
}
