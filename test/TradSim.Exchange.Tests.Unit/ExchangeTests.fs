module ExchangeTests

open Xunit
open Exchange

[<Fact>]    
let ``Library converts "Banana" correctly``() =
    let expected = """I used to be Banana"""
    let actual =  getJsonNetJson "Banana"
    Assert.Equal(expected, actual)