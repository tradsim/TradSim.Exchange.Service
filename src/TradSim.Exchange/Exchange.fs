module TradSim.Exchange

open System
open System.Collections.Generic

type TradeDirection = Buy = 0 | Sell = 1
type OrderStatus = Pending = 0 | PartiallyFilled = 1 | FullyFilled = 2 | OverFilled = 3

type Order = {
    Id : Guid
    Symbol : string
    Price : decimal
    Quantity : int
    OriginalQuantity : int
    Direction : TradeDirection
    Status : OrderStatus
}

let setOrderStatus order =
    if order.Quantity = 0 then { order with Status = OrderStatus.Pending }
    elif order.Quantity < order.OriginalQuantity then { order with Status = OrderStatus.PartiallyFilled }
    elif order.Quantity = order.OriginalQuantity then { order with Status = OrderStatus.FullyFilled }    
    else { order with Status = OrderStatus.OverFilled }

type OrderQuantity = {
    Quantity: int
    Orders: Order list
}

let appendOrderToOrderQuantity entry order=
    match entry with
    | None -> { Quantity = order.OriginalQuantity; Orders = [order]}  
    | Some(e) -> {e with Quantity = e.Quantity + order.OriginalQuantity; Orders= List.append e.Orders [order]}

type OrderPrice = {
    Price:decimal
    Buy: OrderQuantity
    Sell: OrderQuantity 
}

type OrderBook = {
    Symbol: Dictionary<string,OrderPrice list>
}

let createOrderPrice order =
    match order.Direction with
    | TradeDirection.Buy  -> { Price=order.Price; Buy = appendOrderToOrderQuantity None order ; Sell = { Quantity=0; Orders=[]} }
    | TradeDirection.Sell -> { Price=order.Price; Buy = { Quantity=0; Orders=[]} ; Sell = appendOrderToOrderQuantity None order  }
    | _                   -> raise <| new ArgumentOutOfRangeException("Direction",order.Direction,"Direction is not supported!")

let matchOrderPrice price orderPrice  =
    orderPrice.Price = price

let matchOrderPriceList (prices:OrderPrice list) price =
      let filtered = List.filter (matchOrderPrice price) prices
      match List.length filtered with
      | 0 -> None
      | 1 -> Some (filtered.Item 0)
      | _ -> raise <| new Exception("Only one order price should exist!")

// processOrder
//  if book does not contain symbol then addOrderToOrderBook
//  if it contains symbol processSymbolOrder

// let processOrder book (order:Order) =

//     if book.Symbol.ContainsKey(order.Symbol) then
//         match matchOrderPrices (book.Symbol.Item order.Symbol) order with
//         | None book.Symbol.[order.Symbol] <- prices 
//         | Some(p)

//         let prices = processOrderPrices (book.Symbol.Item order.Symbol) order
        
//     else
//         book.Symbol.Add(order.Symbol,[createOrderPrice order])    

