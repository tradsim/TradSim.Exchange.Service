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

[<Fact>]
let ``Append multiple orders to empty order price entry`` () =
    let order1 = { Id=Guid.NewGuid() ; Symbol="TT"; Price=10.0m; Quantity = 10; OriginalQuantity = 10; Direction=TradeDirection.Buy; Status=OrderStatus.FullyFilled }
    let order2 = { Id=Guid.NewGuid() ; Symbol="TT"; Price=10.0m; Quantity = 10; OriginalQuantity = 15; Direction=TradeDirection.Buy; Status=OrderStatus.FullyFilled }
    let entry = appendToPriceEntry (Some (appendToPriceEntry None order1)) order2

    Assert.Equal(25, entry.Quantity)
    Assert.Equal(order1, entry.Orders.Item 0)
    Assert.Equal(order2, entry.Orders.Item 1)