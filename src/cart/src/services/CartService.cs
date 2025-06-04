// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0
using System.Diagnostics;
using System.Threading.Tasks;
using System;
using Grpc.Core;
using cart.cartstore;
using OpenFeature;
using Oteldemo;

namespace cart.services;

public class CartService : Oteldemo.CartService.CartServiceBase
{
    private static readonly Empty Empty = new();
    private readonly Random random = new Random();
    private readonly ICartStore _badCartStore;
    private readonly ICartStore _cartStore;
    private readonly IFeatureClient _featureFlagHelper;

    public CartService(ICartStore cartStore, ICartStore badCartStore, IFeatureClient featureFlagService)
    {
        _badCartStore = badCartStore;
        _cartStore = cartStore;
        _featureFlagHelper = featureFlagService;
    }

    public override async Task<Empty> AddItem(AddItemRequest request, ServerCallContext context)
    {
        var activity = Activity.Current;
        activity?.SetTag("app.user.id", request.UserId);
        activity?.SetTag("app.product.id", request.Item.ProductId);
        activity?.SetTag("app.product.quantity", request.Item.Quantity);

        try
        {
            double failureRate = await _featureFlagHelper.GetDoubleValueAsync("cartFailure", 0.0);
            double randomValue = 9 + (random.NextDouble() * 91);
            double scaledFailureRate = 9 + (failureRate * 91);

            if (randomValue < scaledFailureRate)
            {
                await Task.Delay(random.Next(500, 2500));
                
                    Console.WriteLine($"AddItem - 呼叫_badCartStore 出错");
                    activity?.SetStatus(ActivityStatusCode.Error, "呼叫_badCartStore 出错");
                    throw new RpcException(new Status(StatusCode.Internal, "呼叫_badCartStore 出错"));
            } else {
                await _cartStore.AddItemAsync(request.UserId, request.Item.ProductId, request.Item.Quantity);
            }

            return Empty;
        }
        catch (RpcException ex)
        {
            activity?.AddException(ex);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public override async Task<Cart> GetCart(GetCartRequest request, ServerCallContext context)
    {
        var activity = Activity.Current;
        activity?.SetTag("app.user.id", request.UserId);
        activity?.AddEvent(new("Fetch cart"));

        try
        {
            double failureRate = await _featureFlagHelper.GetDoubleValueAsync("cartFailure", 0.0);
            double randomValue = 9 + (random.NextDouble() * 91);
            double scaledFailureRate = 9 + (failureRate * 91);

            if (randomValue < scaledFailureRate)
            {
                 await Task.Delay(random.Next(100, 500));
            }
            var cart = await _cartStore.GetCartAsync(request.UserId);
            var totalCart = 0;
            foreach (var item in cart.Items)
            {
                totalCart += item.Quantity;
            }
            activity?.SetTag("app.cart.items.count", totalCart);

            return cart;
            
        }
        catch (RpcException ex)
        {
            activity?.AddException(ex);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public override async Task<Empty> EmptyCart(EmptyCartRequest request, ServerCallContext context)
    {
        var activity = Activity.Current;
        activity?.SetTag("app.user.id", request.UserId);
        activity?.AddEvent(new("Empty cart"));

        try
        {
            double failureRate = await _featureFlagHelper.GetDoubleValueAsync("cartFailure", 0.0);
            double randomValue = 9 + (random.NextDouble() * 91);
            double scaledFailureRate = 9 + (failureRate * 91);

            if (randomValue < scaledFailureRate)
            {
                if (randomValue <= 20)
                {
                    await Task.Delay(random.Next(1000, 3001));
                    await _badCartStore.EmptyCartAsync(request.UserId);
                }
                else
                {
                    Console.WriteLine($"EmptyCart - 呼叫_badCartStore 出错");
                    activity?.SetStatus(ActivityStatusCode.Error, "呼叫_badCartStore 出错");
                    throw new RpcException(new Status(StatusCode.Internal, "呼叫_badCartStore 出错"));
                }
            }
            else
            {
                await _cartStore.EmptyCartAsync(request.UserId);
            }
        }
        catch (RpcException ex)
        {
            Activity.Current?.AddException(ex);
            Activity.Current?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }

        return Empty;
    }
}
