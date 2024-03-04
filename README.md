# Texas Hold'em Game Engine

This is Forked from Texas Hold 'em Poker Game Engine.
I like Texas Hold’em and I want use WPF to show it.

## Build status

##2024/2/27
详细描述：GameViewModel中DealToPlayers方法需要在前面加上await Task.Delay(1);否则第一次发牌行为OnDealedToPlayer方法无法进入。<br/>

## 2024/2/26
解决了昨天的bug但不知道什么原理：简单粗暴的在Decks被赋予新值后Delay了10ms<br/>
Decks创建时每个Border都Attach了行为，但是第一个Border没有触发OnDealToPlayers方法(断点未进入)<br/>
希望各位.net高级调试、WPF十五年老兵、C#之父中国分父、不再维护但仍关注WPF的开发人员可以施以援手<br/>


## 2024/2/25 
GameViewModel.cs文件的第76、77行记录了一个莫名其妙的bug <br/>
直接使用CleanTable方法会导致发第一张牌不触发动画，未能找到原因<br/>
有WPF高高手可以帮忙解决一下，我 SOLID 将狠狠 acknowledge 你




[![Build Status](https://nikolayit.visualstudio.com/TexasHoldemGameEngine/_apis/build/status/NikolayIT.TexasHoldemGameEngine?branchName=master)](https://nikolayit.visualstudio.com/TexasHoldemGameEngine/_build/latest?definitionId=18&branchName=master)

## NuGet

<https://www.nuget.org/packages/TexasHoldemGameEngine>
