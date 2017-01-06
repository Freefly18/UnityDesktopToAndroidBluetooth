// By :
// julian m bucknall (2005 - 2009) : http://www.boyet.com/Articles/LockfreeQueue.html
// Steinbitglis (2014) : http://stackoverflow.com/questions/21908174/what-is-the-best-alternative-for-a-concurrent-task-queue-not-using-net-4-0

using System.Threading;

public static class SyncMethods {
	
	public static bool CAS<T>(ref T location, T comparand, T newValue) where T : class {
		return
			(object) comparand ==
				(object) Interlocked.CompareExchange<T>(ref location, newValue, comparand);
	}
}

public class SingleLinkNode<T> {
	public SingleLinkNode<T> Next;
	public T Item;
}

public class LockFreeLinkPool<T> {
	/*public struct SingleLinkNode<U> where U : T {
		// Note; the Next member cannot be a property since
		// it participates in many CAS operations
		public SingleLinkNode<U> Next;
		public U Item;
	}*/
	private SingleLinkNode<T> head;
	
	public LockFreeLinkPool() {
		head = new SingleLinkNode<T>();
	}
	
	public void Push(SingleLinkNode<T> newNode) {
		//newNode.Item = default(T);
		do {
			newNode.Next = head.Next;
		} while (!SyncMethods.CAS<SingleLinkNode<T>>(ref head.Next, newNode.Next, newNode));
		return;
	}
	
	public bool Pop(out SingleLinkNode<T> node) {
		do {
			node = head.Next;
			if (node == null) {
				return false;
			}
		} while (!SyncMethods.CAS<SingleLinkNode<T>>(ref head.Next, node, node.Next));
		return true;
	}
}