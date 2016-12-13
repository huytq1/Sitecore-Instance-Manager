﻿namespace SIM.IO
{
  using System;
  using System.IO;
  using JetBrains.Annotations;
  using SIM.Abstract.IO;
  using SIM.Extensions;

  public class MockFile : MockFileSystemEntry, IFile, IEquatable<MockFile>
  {
    public MockFile(IFileSystem fileSystem, string path, [NotNull] string contents) : base(fileSystem, path)
    {
      Assert.ArgumentNotNullOrEmpty(contents, nameof(contents));

      Stream = GetStream(contents);
      Folder = fileSystem.ParseFolder(Path.GetDirectoryName(FullName));
    }

    public MockFile(IFileSystem fileSystem, string path, [NotNull] Stream stream) : base(fileSystem, path)
    {
      Assert.ArgumentNotNull(stream, nameof(stream));

      Stream = stream;
      Folder = fileSystem.ParseFolder(Path.GetDirectoryName(FullName));
    }

    public MockFile(IFileSystem fileSystem, string path, Func<Stream> openRead = null, Func<Stream> openWrite = null) : base(fileSystem, path)
    {
      OpenReadFunc = openRead;
      OpenWriteFunc = openWrite;
      Folder = fileSystem.ParseFolder(Path.GetDirectoryName(FullName));
    }

    public Func<Stream> OpenReadFunc { get; }

    public Func<Stream> OpenWriteFunc { get; }

    protected Stream Stream { get; set; }

    public IFolder Folder { get; }

    public bool Exists { get; set; } = true;

    public Stream OpenRead()
    {
      if (OpenReadFunc != null)
      {
        return OpenReadFunc().IsNotNull();
      }

      if (Stream == null)
      {
        throw new IOException("File does not have a stream assigned");
      }           
      
      Stream.Seek(0, SeekOrigin.Begin);

      return Stream;
    }

    public Stream OpenWrite()
    {
      return Stream ?? OpenWriteFunc?.Invoke() ?? (Stream = new KeepAliveMemoryStream());
    }

    private static MemoryStream GetStream(string text)
    {               
      var stream = new KeepAliveMemoryStream();
      var writer = new StreamWriter(stream);
      writer.Write(text);
      writer.Flush();
      stream.Position = 0;

      return stream;
    }

    public bool Equals(MockFile other)
    {
      return this.Equals(other, x => x?.FullName);
    }

    public override bool Equals(object obj)
    {
      return this.Equals(obj, x => x?.FullName);
    }

    public override int GetHashCode()
    {
      return this.FullName.GetHashCode();
    }

    public class KeepAliveMemoryStream : MemoryStream
    {
      protected override void Dispose(bool disposing)
      {
        // keep stream open        
      }

      public override void Close()
      {        
      }

      public void DoDispose()
      {
        base.Close();
      }
    }
  }
}