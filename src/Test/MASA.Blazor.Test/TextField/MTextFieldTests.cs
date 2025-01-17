﻿using BlazorComponent;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MASA.Blazor.Test.TextField
{
    [TestClass]
    public class MTextFieldTests : TestBase
    {
        [TestMethod]
        public void RenderShouldHasNoEmptyComponent()
        {
            // Act
            var cut = RenderComponent<MTextField<string>>();
            var components = cut.FindComponents<EmptyComponent>();

            // Assert
            Assert.AreEqual(0, components.Count);
        }

        [TestMethod]
        public void RenderShouldHasClasses()
        {
            // Act
            var cut = RenderComponent<MTextField<string>>();
            var classes = cut.Instance.CssProvider.GetClass();

            // Assert
            Assert.AreEqual("m-input theme--light m-text-field m-text-field--is-booted", classes);
        }

        [TestMethod]
        public void RenderSoloIsSoloShouldBeTrue()
        {
            // Act
            var cut = RenderComponent<MTextField<string>>(props =>
            {
                props.Add(p => p.Solo, true);
            });

            // Assert
            Assert.IsTrue(cut.Instance.IsSolo);
        }

        [TestMethod]
        public async Task HandleOnChangeValueShouldChange()
        {
            // Arrange
            var cut = RenderComponent<MTextField<string>>();
            var args = new ChangeEventArgs()
            {
                Value = "hello"
            };

            // Act
            await cut.Instance.HandleOnChangeAsync(args);

            // Assert
            Assert.AreEqual("hello", cut.Instance.Value);
        }

        [TestMethod]
        public async Task HandleOnChangeValueChangedShouldBeCalled()
        {
            // Arrange
            using var factory = new TestEventCallbackFactory();

            var val = "";
            var cut = RenderComponent<MTextField<string>>(props =>
            {
                props.Add(p => p.ValueChanged, factory.CreateEventCallback<string>(v =>
                {
                    val = v;
                }));
            });

            // Act
            var args = new ChangeEventArgs()
            {
                Value = "hello"
            };
            await factory.Reciever.InvokeAsync(() => cut.Instance.HandleOnChangeAsync(args));

            // Assert
            Assert.AreEqual("hello", cut.Instance.Value);
            Assert.AreEqual("hello", val);
        }
    }
}
