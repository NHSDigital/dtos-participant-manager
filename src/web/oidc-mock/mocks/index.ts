async function initMocks() {
  const { server } = await import("./server");
  server.listen();
}

initMocks();

export {};
