from flask import Flask, render_template, request, redirect, url_for, session, make_response
import requests
import bleach

app = Flask(__name__)
app.secret_key = "verysecretflaskkey123!"

API_BASE = "https://localhost:7003/api"  # adjust to your backend URL

@app.after_request
def add_security_headers(response):
    response.headers['Content-Security-Policy'] = "default-src 'self'; style-src 'self' 'unsafe-inline'"
    response.headers['X-Content-Type-Options'] = 'nosniff'
    response.headers['X-Frame-Options'] = 'DENY'
    return response

@app.route('/')
def index():
    return render_template('index.html')

@app.route('/login', methods=['GET', 'POST'])
def login():
    if request.method == 'POST':
        username = request.form.get('username', '')
        password = request.form.get('password', '')
        totp = request.form.get('totp', '')
        # Call backend API
        try:
            resp = requests.post(f"{API_BASE}/auth/login", json={
                "username": username,
                "password": password,
                "totpCode": totp
            }, verify=False)  # verify=False only for dev
            if resp.ok:
                data = resp.json()
                session['token'] = data['token']
                return redirect(url_for('dashboard'))
            else:
                error = resp.json().get('message', 'Login failed')
                return render_template('login.html', error=error)
        except Exception as e:
            return render_template('login.html', error="Cannot connect to backend")
    return render_template('login.html')

@app.route('/dashboard')
def dashboard():
    if 'token' not in session:
        return redirect(url_for('login'))
    return render_template('dashboard.html')

@app.route('/transfer', methods=['GET', 'POST'])
def transfer():
    if 'token' not in session:
        return redirect(url_for('login'))
    if request.method == 'POST':
        # Apply input validation and sanitization
        account = bleach.clean(request.form.get('account', ''))
        amount_str = request.form.get('amount', '0')
        desc = bleach.clean(request.form.get('description', ''))
        try:
            amount = float(amount_str)
        except ValueError:
            return render_template('transfer.html', error="Invalid amount.")
        headers = {"Authorization": f"Bearer {session['token']}"}
        try:
            resp = requests.post(f"{API_BASE}/transaction/transfer", json={
                "accountNumber": account,
                "amount": amount,
                "description": desc
            }, headers=headers, verify=False)
            if resp.ok:
                return render_template('transfer.html', message="Transfer successful!")
            else:
                return render_template('transfer.html', error=resp.text)
        except Exception:
            return render_template('transfer.html', error="Service unavailable")
    return render_template('transfer.html')

@app.route('/comments', methods=['GET', 'POST'])
def comments():
    # This page demonstrates the XSS fix.
    # Stored comments are kept in a simple list (in memory, no DB for demo)
    if 'comments' not in session:
        session['comments'] = []
    if request.method == 'POST':
        user_comment = request.form.get('comment', '')
        # Clean the comment using bleach (removes script tags)
        clean_comment = bleach.clean(user_comment)
        session['comments'].append(clean_comment)
        session.modified = True
    return render_template('comments.html', comments=session.get('comments', []))

@app.route('/logout')
def logout():
    session.clear()
    return redirect(url_for('index'))

if __name__ == '__main__':
    app.run(debug=True, ssl_context='adhoc')  # uses self-signed cert for HTTPS