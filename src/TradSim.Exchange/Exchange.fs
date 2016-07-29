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

type OrderPrice = {
    Price:decimal
    Buy: OrderQuantity
    Sell: OrderQuantity
}

type OrderBook = {
    Symbol: Dictionary<string,OrderPrice list>
}

let appendOrderToOrderQuantity order quantity=
    {quantity with Quantity = quantity.Quantity + order.OriginalQuantity; Orders= List.append quantity.Orders [order]}
     

let createOrderPrice order =
    match order.Direction with
    | TradeDirection.Buy  -> { Price=order.Price; Buy = appendOrderToOrderQuantity order { Quantity=0; Orders=[]}  ; Sell = { Quantity=0; Orders=[]} }
    | TradeDirection.Sell -> { Price=order.Price; Buy = { Quantity=0; Orders=[]} ; Sell = appendOrderToOrderQuantity order { Quantity=0; Orders=[]} }
    | _                   -> raise <| new ArgumentOutOfRangeException("Direction",order.Direction,"Direction is not supported!")

let appendOrderToOrderPrice order price =
    match order.Direction with
    | TradeDirection.Buy  -> { price with Buy = appendOrderToOrderQuantity order price.Buy  }
    | TradeDirection.Sell -> { price with Sell = appendOrderToOrderQuantity order price.Sell }
    | _                   -> raise <| new ArgumentOutOfRangeException("Direction",order.Direction,"Direction is not supported!")

let addOrderToOrderBook (order:Order) book =

    let mutable tradeAble = false
    if book.Symbol.ContainsKey(order.Symbol) then
        let mutable found = false
        let prices = List.map (fun pr -> if pr.Price = order.Price then found <- true                                                                        
                                                                        appendOrderToOrderPrice order pr                                                                                                                                                                                                
                                         else pr) book.Symbol.[order.Symbol]

        if found then
            tradeAble <- true
            book.Symbol.[order.Symbol] <- prices
        else 
            book.Symbol.[order.Symbol] <- createOrderPrice order :: prices |> List.sortBy (fun price -> price.Price)        
    else
        book.Symbol.Add(order.Symbol,[createOrderPrice order])        
         
    // Send order created event

    if tradeAble then Some order.Price else None
                                    

// let processOrder book (order:Order) =

//     if book.Symbol.ContainsKey(order.Symbol) then
//         match matchOrderPrices (book.Symbol.Item order.Symbol) order with
//         | None -> book.Symbol.[order.Symbol] <- prices 
//         | Some(p) ->

//         let prices = processOrderPrices (book.Symbol.Item order.Symbol) order
        
//     else
//         book.Symbol.Add(order.Symbol,[createOrderPrice order])    


// processOrder
//  Add order to book
//  Trade Order

