using System.Text.Json.Serialization;
using RinhaBackend.Api.Data;
using RinhaBackend.Api.Endpoints;
using RinhaBackend.Api.Models;

namespace RinhaBackend.Api;

[JsonSourceGenerationOptions(
    WriteIndented = false,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    GenerationMode = JsonSourceGenerationMode.Default)]
[JsonSerializable(typeof(GrainResponse))]
[JsonSerializable(typeof(TransacaoRequest))]
[JsonSerializable(typeof(ContaSaldo))]
[JsonSerializable(typeof(ContaSaldoExtrato))]
[JsonSerializable(typeof(ContaExtrato))]
[JsonSerializable(typeof(Transacao))]
[JsonSerializable(typeof(TransacaoEntidade))]
[JsonSerializable(typeof(Conta))]
internal partial class JsonContext : JsonSerializerContext { }
