module TradSim.ExchangeTests

open System
open System.Collections.Generic
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
    let entry = appendOrderToOrderQuantity (Some (appendOrderToOrderQuantity None order1)) order2

    Assert.Equal(25, entry.Quantity)
    Assert.Equal(order1, entry.Orders.Item 0)
    Assert.Equal(order2, entry.Orders.Item 1)

[<Theory>]
[<InlineData(TradeDirection.Buy)>]
[<InlineData(TradeDirection.Sell)>]
let ``Create OrderPrice`` direction =
    let order = { Id=Guid.NewGuid() ; Symbol="TT"; Price=10.0m; Quantity = 10; OriginalQuantity = 10; Direction=direction; Status=OrderStatus.FullyFilled }
    let orderPrice = createOrderPrice order

    Assert.Equal(10.0m, orderPrice.Price)
    if direction = TradeDirection.Sell then 
        Assert.Equal(10,orderPrice.Sell.Quantity)
        Assert.Equal(order,orderPrice.Sell.Orders.Item 0)
    else
        Assert.Equal(10,orderPrice.Buy.Quantity)
        Assert.Equal(order,orderPrice.Buy.Orders.Item 0)

[<Fact>]
let ``Create OrderPrice with invalid direction throws`` () =
    let order = { Id=Guid.NewGuid() ; Symbol="TT"; Price=10.0m; Quantity = 10; OriginalQuantity = 10; Direction= enum<TradeDirection> 10; Status=OrderStatus.FullyFilled }

    Assert.Throws<ArgumentOutOfRangeException>(fun () -> createOrderPrice order |> ignore) |> ignore
    

[<Theory>]
[<InlineData(10.0,true)>]
[<InlineData(11.0,false)>]
let ``Match order price`` price expectedMatch  =
    let order = { Id=Guid.NewGuid() ; Symbol="TT"; Price=price; Quantity = 10; OriginalQuantity = 10; Direction= TradeDirection.Buy ; Status=OrderStatus.FullyFilled }
    let orderPrice = { createOrderPrice order with Price=10.0m}

    let actualMatch = matchOrderPrice order.Price orderPrice

    Assert.Equal(expectedMatch , actualMatch)

[<Fact>]
let ``Match order price list throws on duplicate results`` ()  =
    let order = { Id=Guid.NewGuid() ; Symbol="TT"; Price=10.0m; Quantity = 10; OriginalQuantity = 10; Direction= TradeDirection.Buy ; Status=OrderStatus.FullyFilled }
    let list = [createOrderPrice order;createOrderPrice order]

    Assert.Throws<Exception>(fun () -> matchOrderPriceList list order.Price |> ignore) |> ignore

[<Theory>]
[<InlineData(10.0,true)>]
[<InlineData(12.0,false)>]
let ``Match order price list`` price (expectedMatch:bool)  =
    let order = { Id=Guid.NewGuid() ; Symbol="TT"; Price=price; Quantity = 10; OriginalQuantity = 10; Direction= TradeDirection.Buy ; Status=OrderStatus.FullyFilled }
    let list = [createOrderPrice {order with Price=10.0m};createOrderPrice {order with Price=11.0m}]

    match matchOrderPriceList list order.Price with
    | None -> Assert.False(expectedMatch)
    | Some(p) -> Assert.True(expectedMatch)