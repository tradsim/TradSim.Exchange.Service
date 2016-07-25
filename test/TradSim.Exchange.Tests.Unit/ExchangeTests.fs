module TradSim.ExchangeTests

open System
open Xunit
open TradSim.Exchange

[<Theory>]
[<InlineData(0, OrderStatus.Pending)>]
[<InlineData(5, OrderStatus.PartiallyFilled)>]
[<InlineData(10, OrderStatus.FullyFilled)>]
[<InlineData(11, OrderStatus.OverFilled)>]
let ``Set Order Status Tests`` quantity expectedStatus =
    let order = { Id=Guid.NewGuid() ; Symbol="TT"; Price=10.0m; Quantity = quantity; OriginalQuantity = 10; Direction=TradeDirection.Buy; Status=OrderStatus.FullyFilled }
    let actual =  setOrderStatus order

    Assert.Equal(expectedStatus, actual.Status)
