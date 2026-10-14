import { useEffect, useState } from 'react';

// Change this if Visual Studio starts the API on another port.
const API_BASE_URL = 'https://localhost:7161/api/work-requests';
const statuses = ['New', 'InProgress', 'Blocked', 'Completed'];
const priorities = ['Low', 'Medium', 'High'];

const emptyForm = {
  title: '',
  clientName: '',
  description: '',
  priority: 'Medium',
  status: 'New',
  dueDate: '',
  notes: ''
};

export default function App() {
  const [requests, setRequests] = useState([]);
  const [status, setStatus] = useState('');
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [form, setForm] = useState(emptyForm);
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [noteDrafts, setNoteDrafts] = useState({});

  async function loadRequests() {
    setLoading(true);
    setError('');
    try {
      const params = new URLSearchParams({ page, pageSize: 10 });
      if (status) params.set('status', status);
      if (search.trim()) params.set('search', search.trim());

      const response = await fetch(`${API_BASE_URL}?${params}`);
      const data = await response.json();
      if (!response.ok) throw new Error(data?.error?.message || 'Could not load work requests.');

      setRequests(data.items);
      setTotalCount(data.totalCount);
    } catch (err) {
      setError(`${err.message} Check that the API is running and the URL is correct.`);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadRequests();
    // Reload when the filter, search, or page changes.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [status, search, page]);

  function updateForm(event) {
    setForm({ ...form, [event.target.name]: event.target.value });
  }

  async function createRequest(event) {
    event.preventDefault();
    setSaving(true);
    setError('');
    try {
      const response = await fetch(API_BASE_URL, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ ...form, dueDate: new Date(form.dueDate).toISOString() })
      });
      const data = await response.json();
      if (!response.ok) throw new Error(data?.error?.message || 'Could not create request.');

      setForm(emptyForm);
      setPage(1);
      await loadRequests();
    } catch (err) {
      setError(err.message);
    } finally {
      setSaving(false);
    }
  }

  async function changeStatus(id, newStatus) {
    setError('');
    try {
      const response = await fetch(`${API_BASE_URL}/${id}/status`, {
        method: 'PATCH',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ status: newStatus })
      });
      const data = await response.json();
      if (!response.ok) throw new Error(data?.error?.message || 'Could not update status.');
      await loadRequests();
    } catch (err) {
      setError(err.message);
    }
  }

  async function addNote(id) {
    const text = (noteDrafts[id] || '').trim();
    if (!text) return;
    setError('');
    try {
      const response = await fetch(`${API_BASE_URL}/${id}/notes`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ text })
      });
      const data = await response.json();
      if (!response.ok) throw new Error(data?.error?.message || 'Could not add note.');
      setNoteDrafts({ ...noteDrafts, [id]: '' });
    } catch (err) {
      setError(err.message);
    }
  }

  const totalPages = Math.max(1, Math.ceil(totalCount / 10));

  return (
    <main className="container">
      <header>
        <div>
          <p className="eyebrow">FLEXGCC · PRACTICAL ASSIGNMENT</p>
          <h1>Work Request Tracker</h1>
          <p className="subheading">Create, find and track client work requests.</p>
        </div>
      </header>

      <section className="panel">
        <h2>Create a request</h2>
        <form className="form-grid" onSubmit={createRequest}>
          <label>Title *
            <input name="title" value={form.title} onChange={updateForm} minLength="3" maxLength="150" required />
          </label>
          <label>Client name *
            <input name="clientName" value={form.clientName} onChange={updateForm} minLength="2" maxLength="120" required />
          </label>
          <label>Priority *
            <select name="priority" value={form.priority} onChange={updateForm} required>
              {priorities.map(item => <option key={item}>{item}</option>)}
            </select>
          </label>
          <label>Status *
            <select name="status" value={form.status} onChange={updateForm} required>
              {statuses.map(item => <option key={item} value={item}>{item}</option>)}
            </select>
          </label>
          <label>Due date *
            <input type="date" name="dueDate" value={form.dueDate} onChange={updateForm} required />
          </label>
          <label className="wide">Description *
            <textarea name="description" value={form.description} onChange={updateForm} minLength="5" maxLength="2000" required rows="3" />
          </label>
          <label className="wide">Optional note
            <input name="notes" value={form.notes} onChange={updateForm} maxLength="1000" />
          </label>
          <div className="wide">
            <button disabled={saving}>{saving ? 'Saving…' : 'Create request'}</button>
          </div>
        </form>
      </section>

      <section className="panel">
        <div className="list-heading">
          <div>
            <h2>Work requests</h2>
            <p className="muted">{totalCount} total request(s)</p>
          </div>
          <div className="filters">
            <label>Search
              <input placeholder="Title or client" value={search} onChange={e => { setSearch(e.target.value); setPage(1); }} />
            </label>
            <label>Status
              <select value={status} onChange={e => { setStatus(e.target.value); setPage(1); }}>
                <option value="">All statuses</option>
                {statuses.map(item => <option key={item} value={item}>{item}</option>)}
              </select>
            </label>
          </div>
        </div>

        {error && <p className="error" role="alert">{error}</p>}
        {loading ? <p>Loading work requests…</p> : requests.length === 0 ? (
          <p className="empty">No requests found. Create one above or change your filters.</p>
        ) : (
          <div className="table-wrap">
            <table>
              <thead><tr><th>Request</th><th>Client</th><th>Priority</th><th>Due date</th><th>Status</th><th>Add note</th></tr></thead>
              <tbody>
                {requests.map(item => (
                  <tr key={item.id}>
                    <td><strong>#{item.id} · {item.title}</strong><small>Created {new Date(item.createdDate).toLocaleDateString()}</small></td>
                    <td>{item.clientName}</td>
                    <td>{item.priority}</td>
                    <td>{new Date(item.dueDate).toLocaleDateString()}</td>
                    <td>
                      <select aria-label={`Status for request ${item.id}`} value={item.status} onChange={e => changeStatus(item.id, e.target.value)}>
                        {statuses.map(value => <option key={value} value={value}>{value}</option>)}
                      </select>
                    </td>
                    <td>
                      <div className="note-control">
                        <input aria-label={`Note for request ${item.id}`} placeholder="Write a note" value={noteDrafts[item.id] || ''} onChange={e => setNoteDrafts({ ...noteDrafts, [item.id]: e.target.value })} maxLength="1000" />
                        <button className="secondary" type="button" onClick={() => addNote(item.id)} disabled={!(noteDrafts[item.id] || '').trim()}>Add</button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
        <div className="pagination">
          <button className="secondary" disabled={page <= 1} onClick={() => setPage(page - 1)}>Previous</button>
          <span>Page {page} of {totalPages}</span>
          <button className="secondary" disabled={page >= totalPages} onClick={() => setPage(page + 1)}>Next</button>
        </div>
      </section>
      <footer>Built with React and ASP.NET Core Web API</footer>
    </main>
  );
}
