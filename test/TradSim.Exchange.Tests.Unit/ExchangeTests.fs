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
    let order3 = { Id=Guid.NewGuid() ; Symbol="TT"; Price=10.0m; Quantity = 10; OriginalQuantity = 25; Direction=TradeDirection.Buy; Status=OrderStatus.FullyFilled }
    let entry = { Quantity=0; Orders=[]} |> appendOrderToOrderQuantity order1 |> appendOrderToOrderQuantity order2 |> appendOrderToOrderQuantity order3

    Assert.Equal(50, entry.Quantity)
    Assert.Equal(order1, entry.Orders.[0])
    Assert.Equal(order2, entry.Orders.[1])
    Assert.Equal(order3, entry.Orders.[2])

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

[<Fact>]
let ``appendOrderToOrderPrice: add one buy to existing buy order`` () =
    let order1 = { Id=Guid.NewGuid() ; Symbol="TT"; Price=10.0m; Quantity = 10; OriginalQuantity = 10; Direction= TradeDirection.Buy ; Status=OrderStatus.FullyFilled }
    let order2 = { Id=Guid.NewGuid() ; Symbol="TT"; Price=10.0m; Quantity = 10; OriginalQuantity = 15; Direction= TradeDirection.Buy ; Status=OrderStatus.FullyFilled }
    let price = createOrderPrice order1 |> appendOrderToOrderPrice order2

    Assert.Equal(order1.Price,price.Price)
    Assert.Equal(25,price.Buy.Quantity)
    Assert.Equal(order1,price.Buy.Orders.[0])
    Assert.Equal(order2,price.Buy.Orders.[1])

[<Fact>]
let ``appendOrderToOrderPrice: add one sell to existing buy order`` () =
    let order1 = { Id=Guid.NewGuid() ; Symbol="TT"; Price=10.0m; Quantity = 10; OriginalQuantity = 10; Direction= TradeDirection.Buy ; Status=OrderStatus.FullyFilled }
    let order2 = { Id=Guid.NewGuid() ; Symbol="TT"; Price=10.0m; Quantity = 10; OriginalQuantity = 15; Direction= TradeDirection.Sell ; Status=OrderStatus.FullyFilled }
    let price = createOrderPrice order1 |> appendOrderToOrderPrice order2

    Assert.Equal(order1.Price,price.Price)
    Assert.Equal(10,price.Buy.Quantity)
    Assert.Equal(15,price.Sell.Quantity)
    Assert.Equal(order1,price.Buy.Orders.[0])
    Assert.Equal(order2,price.Sell.Orders.[0])

[<Fact>]
let ``addOrderToOrderBook: Add Order to empty`` () =
    let order = { Id=Guid.NewGuid() ; Symbol="TT"; Price=10.0m; Quantity = 10; OriginalQuantity = 10; Direction= TradeDirection.Buy; Status=OrderStatus.FullyFilled }
    let book = { Symbol= new Dictionary<string,OrderPrice list>() }

    let toTrade = addOrderToOrderBook order book
    Assert.False(toTrade)
    let symbolPrice =book.Symbol.["TT"] 
    Assert.Equal(1,symbolPrice.Length)
    Assert.Equal(order.Price,symbolPrice.[0].Price)
    Assert.Equal(order.OriginalQuantity,symbolPrice.[0].Buy.Quantity)
    Assert.Equal(order,symbolPrice.[0].Buy.Orders.[0])

[<Fact>]
let ``addOrderToOrderBook: Add two buy orders`` () =
    let order1 = { Id=Guid.NewGuid() ; Symbol="TT"; Price=10.0m; Quantity = 10; OriginalQuantity = 10; Direction= TradeDirection.Buy; Status=OrderStatus.FullyFilled }
    let order2 = { Id=Guid.NewGuid() ; Symbol="TT"; Price=10.0m; Quantity = 10; OriginalQuantity = 15; Direction= TradeDirection.Buy; Status=OrderStatus.FullyFilled }
    let book = { Symbol= new Dictionary<string,OrderPrice list>() }

    let toTrade = addOrderToOrderBook order1 book
    let toTrade = addOrderToOrderBook order2 book

    Assert.True(toTrade)
    let symbolPrice =book.Symbol.["TT"] 
    Assert.Equal(1,symbolPrice.Length)
    Assert.Equal(order1.Price,symbolPrice.[0].Price)
    Assert.Equal(25,symbolPrice.[0].Buy.Quantity)
    Assert.Equal(order1,symbolPrice.[0].Buy.Orders.[0])
    Assert.Equal(order2,symbolPrice.[0].Buy.Orders.[1])

[<Fact>]
let ``addOrderToOrderBook: Add one buy one sell same price`` () =
    let order1 = { Id=Guid.NewGuid() ; Symbol="TT"; Price=10.0m; Quantity = 10; OriginalQuantity = 10; Direction= TradeDirection.Buy; Status=OrderStatus.FullyFilled }
    let order2 = { Id=Guid.NewGuid() ; Symbol="TT"; Price=10.0m; Quantity = 10; OriginalQuantity = 15; Direction= TradeDirection.Sell; Status=OrderStatus.FullyFilled }
    let book = { Symbol = new Dictionary<string,OrderPrice list>() }
    let toTrade = addOrderToOrderBook order1 book    
    let toTrade = addOrderToOrderBook order2 book
    
    Assert.True(toTrade)
    let symbolPrices = book.Symbol.["TT"]
    Assert.Equal(1,symbolPrices.Length)
    let symbolPrice = symbolPrices.[0]
    Assert.Equal(order1.Price,symbolPrice.Price)
    Assert.Equal(10,symbolPrice.Buy.Quantity)
    Assert.Equal(15,symbolPrice.Sell.Quantity)
    Assert.Equal(order1,symbolPrice.Buy.Orders.[0])
    Assert.Equal(order2,symbolPrice.Sell.Orders.[0])

[<Fact>]
let ``addOrderToOrderBook: Add one buy one sell at 12 and one buy one sell at 11`` () =
    
    let orderSell12 = { Id=Guid.NewGuid() ; Symbol="TT"; Price=12.0m; Quantity = 10; OriginalQuantity = 1; Direction= TradeDirection.Sell; Status=OrderStatus.FullyFilled }
    let orderBuy12 = { Id=Guid.NewGuid() ; Symbol="TT"; Price=12.0m; Quantity = 10; OriginalQuantity = 2; Direction= TradeDirection.Buy; Status=OrderStatus.FullyFilled }
    let orderSell11 = { Id=Guid.NewGuid() ; Symbol="TT"; Price=11.0m; Quantity = 10; OriginalQuantity = 3; Direction= TradeDirection.Sell; Status=OrderStatus.FullyFilled }
    let orderBuy11 = { Id=Guid.NewGuid() ; Symbol="TT"; Price=11.0m; Quantity = 10; OriginalQuantity = 4; Direction= TradeDirection.Buy; Status=OrderStatus.FullyFilled }
    
    let book = { Symbol = new Dictionary<string,OrderPrice list>() }
    let toTrade = addOrderToOrderBook orderSell12 book    
    let toTrade = addOrderToOrderBook orderBuy11 book
    let toTrade = addOrderToOrderBook orderBuy12 book
    let toTrade = addOrderToOrderBook orderSell11 book
    
    Assert.True(toTrade)
    let symbolPrices = book.Symbol.["TT"]
    Assert.Equal(2,symbolPrices.Length)
    let symbolPrice11 = symbolPrices.[0]
    Assert.Equal(11.0m,symbolPrice11.Price)
    Assert.Equal(4, symbolPrice11.Buy.Quantity)
    Assert.Equal(3, symbolPrice11.Sell.Quantity)
    Assert.Equal(orderBuy11,symbolPrice11.Buy.Orders.[0])
    Assert.Equal(orderSell11,symbolPrice11.Sell.Orders.[0])

    let symbolPrice12 = symbolPrices.[1]
    Assert.Equal(12.0m,symbolPrice12.Price)
    Assert.Equal(2, symbolPrice12.Buy.Quantity)
    Assert.Equal(1, symbolPrice12.Sell.Quantity)
    Assert.Equal(orderBuy12,symbolPrice12.Buy.Orders.[0])
    Assert.Equal(orderSell12,symbolPrice12.Sell.Orders.[0])

[<Fact>]
let ``tradeOrders: trade equal orders`` () =
    
    let orderSell = { Id=Guid.NewGuid() ; Symbol="TT"; Price=12.0m; Quantity = 10; OriginalQuantity = 10; Direction= TradeDirection.Sell; Status=OrderStatus.FullyFilled }
    let orderBuy = { Id=Guid.NewGuid() ; Symbol="TT"; Price=12.0m; Quantity = 10; OriginalQuantity = 10; Direction= TradeDirection.Buy; Status=OrderStatus.FullyFilled }

    let (orderBuyTraded,orderSellTraded) = tradeOrders orderBuy orderSell
    Assert.Equal(0,orderBuyTraded.Quantity)
    Assert.Equal(0,orderSellTraded.Quantity)

[<Fact>]
let ``tradeOrders: buy > sell`` () =
    
    let orderSell = { Id=Guid.NewGuid() ; Symbol="TT"; Price=12.0m; Quantity = 10; OriginalQuantity = 10; Direction= TradeDirection.Sell; Status=OrderStatus.FullyFilled }
    let orderBuy = { Id=Guid.NewGuid() ; Symbol="TT"; Price=12.0m; Quantity = 12; OriginalQuantity = 10; Direction= TradeDirection.Buy; Status=OrderStatus.FullyFilled }

    let (orderBuyTraded,orderSellTraded) = tradeOrders orderBuy orderSell
    Assert.Equal(2,orderBuyTraded.Quantity)
    Assert.Equal(0,orderSellTraded.Quantity)

[<Fact>]
let ``tradeOrders: sell > buy`` () =
    
    let orderSell = { Id=Guid.NewGuid() ; Symbol="TT"; Price=12.0m; Quantity = 15; OriginalQuantity = 10; Direction= TradeDirection.Sell; Status=OrderStatus.FullyFilled }
    let orderBuy = { Id=Guid.NewGuid() ; Symbol="TT"; Price=12.0m; Quantity = 12; OriginalQuantity = 10; Direction= TradeDirection.Buy; Status=OrderStatus.FullyFilled }

    let (orderBuyTraded,orderSellTraded) = tradeOrders orderBuy orderSell
    Assert.Equal(0,orderBuyTraded.Quantity)
    Assert.Equal(3,orderSellTraded.Quantity)
    