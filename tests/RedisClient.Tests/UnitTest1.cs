using Moq;
using StackExchange.Redis;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using RedisClient.Models;
using RedisClient.Services;

namespace RedisClientLibrary.Tests
{
    public class RCPRedisClientTests
    {
        private readonly Mock<IOptions<RCPRedisOptions>> _mockOptions;
        private readonly Mock<IConnectionMultiplexer> _mockConnectionMultiplexer;
        private readonly Mock<IDatabase> _mockDatabase;

        private readonly RcpRedisClient _redisClient;

        public RCPRedisClientTests()
        {
            _mockOptions = new Mock<IOptions<RCPRedisOptions>>();
            _mockConnectionMultiplexer = new Mock<IConnectionMultiplexer>();
            _mockDatabase = new Mock<IDatabase>();

            _mockOptions.Setup(x => x.Value).Returns(new RCPRedisOptions { ConnectionString = "localhost" });
            _mockConnectionMultiplexer.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_mockDatabase.Object);
            _mockConnectionMultiplexer.Setup(x => x.IsConnected).Returns(true);

            _redisClient = new RcpRedisClient(_mockOptions.Object)
            {
                // Configurações necessárias
            };

            // Injetar o mock ConnectionMultiplexer na classe RCPRedisClient
            typeof(RcpRedisClient).GetField("_redis", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_redisClient, _mockConnectionMultiplexer.Object);
        }

        [Fact]
        public async Task GetValueAsync_ShouldReturnValidResponse_WhenKeyExists()
        {
            // Arrange
            var key = "existingKey";
            var value = "testValue";
            _mockDatabase.Setup(x => x.StringGetAsync(key, CommandFlags.None)).ReturnsAsync((RedisValue)value);

            // Act
            var response = await _redisClient.GetValueAsync<string>(key);

            // Assert
            Assert.True(response.IsValid);
            Assert.Equal(value, response.Data);
            Assert.Empty(response.Errors);
        }

        [Fact]
        public async Task GetValueAsync_ShouldReturnInvalidResponse_WhenKeyDoesNotExist()
        {
            // Arrange
            var key = "nonExistingKey";
            _mockDatabase.Setup(x => x.StringGetAsync(key, CommandFlags.None)).ReturnsAsync(RedisValue.Null);

            // Act
            var response = await _redisClient.GetValueAsync<string>(key);

            // Assert
            Assert.False(response.IsValid);
            Assert.Null(response.Data);
            Assert.Contains("Key not found or value is null", response.Errors);
        }

        //[Fact]
        //public async Task SetValueAsync_ShouldSetKeyValueInDatabase()
        //{
        //    // Arrange
        //    var key = "testKey";
        //    var value = "testValue";
        //    _mockDatabase.Setup(x => x.StringSetAsync(key, It.IsAny<RedisValue>(), null, When.Always, CommandFlags.None)).ReturnsAsync(true);

        //    // Act
        //    await _redisClient.SetValueAsync(key, value);

        //    // Assert
        //    _mockDatabase.Verify(x => x.StringSetAsync(key, It.Is<RedisValue>(v => v.ToString() == JsonSerializer.Serialize(value)), null, When.Always, CommandFlags.None), Times.Once);
        //}

        [Fact]
        public async Task TryGetValueAsync_ShouldReturnValidResponse_WhenKeyExists()
        {
            // Arrange
            var key = "existingKey";
            var value = "testValue";
            _mockDatabase.Setup(x => x.StringGetAsync(key, CommandFlags.None)).ReturnsAsync((RedisValue)value);

            // Act
            var response = await _redisClient.TryGetValueAsync<string>(key);

            // Assert
            Assert.True(response.IsValid);
            Assert.Equal(value, response.Data);
            Assert.Empty(response.Errors);
        }

        [Fact]
        public async Task TryGetValueAsync_ShouldReturnInvalidResponse_WhenKeyDoesNotExist()
        {
            // Arrange
            var key = "nonExistingKey";
            _mockDatabase.Setup(x => x.StringGetAsync(key, CommandFlags.None)).ReturnsAsync(RedisValue.Null);

            // Act
            var response = await _redisClient.TryGetValueAsync<string>(key);

            // Assert
            Assert.False(response.IsValid);
            Assert.Null(response.Data);
            Assert.Contains("Key not found or value is null", response.Errors);
        }

        [Fact]
        public async Task GetValueAsync_ShouldInvalidateCache_WhenValueIsNull()
        {
            // Arrange
            var key = "keyWithNullValue";
            _mockDatabase.Setup(x => x.StringGetAsync(key, CommandFlags.None)).ReturnsAsync("null");

            // Act
            var response = await _redisClient.GetValueAsync<string>(key);

            // Assert
            Assert.False(response.IsValid);
            Assert.Null(response.Data);
            Assert.Contains("Key not found or value is null", response.Errors);
            _mockDatabase.Verify(x => x.KeyDeleteAsync(key, CommandFlags.None), Times.Once);
        }

        [Fact]
        public async Task HandleDeserialization_ShouldReturnInvalidResponse_WhenDataTypeIsInvalid()
        {
            // Arrange
            var key = "invalidDataTypeKey";
            var invalidJson = "{invalidJson}";
            _mockDatabase.Setup(x => x.StringGetAsync(key, CommandFlags.None)).ReturnsAsync((RedisValue)invalidJson);

            // Act
            var response = await _redisClient.GetValueAsync<int>(key);

            // Assert
            Assert.False(response.IsValid);
            Assert.Null(response.Data);
            Assert.Contains("Invalid data type", response.Errors);
        }
    }
}
