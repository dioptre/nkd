﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EntityFramework.Test
{
  public static class ExceptionAssert
  {
    public static void Throws<TException>(Action action)
        where TException : Exception
    {
      ExceptionAssert.Throws(typeof(TException), action);
    }

    public static void Throws(Type expectedExceptionType, Action action)
    {
      try
      {
        action();
      }
      catch (Exception ex)
      {
        Assert.IsInstanceOfType(ex, expectedExceptionType);
        return;
      }

      Assert.Fail(string.Format("No exception thrown.  Expected exception type of {0}.", expectedExceptionType.Name));
    }
  }
}
