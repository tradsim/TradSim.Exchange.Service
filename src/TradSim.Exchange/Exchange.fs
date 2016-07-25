module TradSim.Exchange

open System

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

type OrderPriceEntry = {
    Quantity: int
    Orders: Order list
}

let appendToPriceEntry entry order=
    match entry with
    | None -> { Quantity = order.OriginalQuantity; Orders = [order]}  
    | Some(e) -> {e with Quantity = e.Quantity + order.OriginalQuantity; Orders= List.append e.Orders [order]}