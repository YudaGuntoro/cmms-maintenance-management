"use client";

import { useCallback, useEffect, useState } from "react";
import { apiGet, apiPost, apiPut } from "./api";
import { formatDateTime } from "./format";
import { Icon } from "./icons";
import { Card, CardHeader, Feedback, TablePanel, TextInput } from "./ui";
import type { TelegramChatResponse, TelegramSettingsResponse } from "./types";

export default function SettingsPage() {
  const [settings, setSettings] = useState<TelegramSettingsResponse | null>(null);
  const [botToken, setBotToken] = useState("");
  const [chatId, setChatId] = useState("");
  const [enabled, setEnabled] = useState(false);
  const [testMessage, setTestMessage] = useState("CMMS notification test from Nusakarya Digital Solution.");
  const [chats, setChats] = useState<TelegramChatResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [testing, setTesting] = useState(false);
  const [loadingChats, setLoadingChats] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const loadSettings = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await apiGet<TelegramSettingsResponse>("/api/settings/telegram");
      setSettings(data);
      setChatId(data?.chat_id || "");
      setEnabled(Boolean(data?.is_enabled));
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load settings.");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void loadSettings();
  }, [loadSettings]);

  async function saveSettings(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setSaving(true);
    setError(null);
    setSuccess(null);
    try {
      const data = await apiPut<TelegramSettingsResponse>("/api/settings/telegram", {
        bot_token: botToken.trim() || null,
        chat_id: chatId.trim() || null,
        is_enabled: enabled,
      });
      setSettings(data);
      setBotToken("");
      setSuccess("Telegram settings saved.");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to save settings.");
    } finally {
      setSaving(false);
    }
  }

  async function sendTestMessage() {
    setTesting(true);
    setError(null);
    setSuccess(null);
    try {
      await apiPost<string>("/api/settings/telegram/test", { message: testMessage });
      setSuccess("Telegram test message sent.");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to send Telegram test message.");
    } finally {
      setTesting(false);
    }
  }

  async function loadChats() {
    setLoadingChats(true);
    setError(null);
    setSuccess(null);
    try {
      const data = await apiGet<TelegramChatResponse[]>("/api/settings/telegram/chats");
      setChats(data || []);
      setSuccess("Telegram chats loaded.");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load Telegram chats.");
    } finally {
      setLoadingChats(false);
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <p className="text-xs font-semibold uppercase tracking-[0.18em] text-brand-600 dark:text-brand-400">Configuration</p>
          <h1 className="mt-2 text-2xl font-semibold text-gray-900 dark:text-white">Settings</h1>
          <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">Telegram group notification setup</p>
        </div>
        <button className="secondary-button" disabled={loading} onClick={() => void loadSettings()} type="button">
          <Icon name="refresh" />
          Refresh
        </button>
      </div>

      <Feedback error={error} success={success} />

      <div className="grid grid-cols-1 gap-6 xl:grid-cols-[1fr_420px]">
        <Card>
          <CardHeader description={settings?.has_bot_token ? `Current token: ${settings.bot_token_preview}` : "Bot token is not configured"} title="Telegram Bot" />
          <form className="grid grid-cols-1 gap-4 p-5 md:grid-cols-2" onSubmit={(event) => void saveSettings(event)}>
            <TextInput
              helper={settings?.has_bot_token ? "Leave empty to keep current token" : undefined}
              label="Bot Token"
              name="bot_token"
              onChange={(event) => setBotToken(event.target.value)}
              placeholder={settings?.has_bot_token ? settings.bot_token_preview || "Configured" : "123456:ABC"}
              value={botToken}
            />
            <TextInput
              label="Group Chat ID"
              name="chat_id"
              onChange={(event) => setChatId(event.target.value)}
              placeholder="-1001234567890"
              value={chatId}
            />
            <label className="flex items-center gap-3 rounded-lg border border-gray-200 bg-white px-4 py-3 dark:border-gray-700 dark:bg-gray-900 md:col-span-2">
              <input
                checked={enabled}
                className="h-4 w-4 rounded border-gray-300 text-brand-500 focus:ring-brand-500 dark:border-gray-600"
                onChange={(event) => setEnabled(event.target.checked)}
                type="checkbox"
              />
              <span>
                <span className="block text-sm font-semibold text-gray-900 dark:text-white">Enable Telegram Notification</span>
                <span className="mt-1 block text-xs text-gray-500 dark:text-gray-400">Problem Report dan Work Order baru akan dikirim ke group.</span>
              </span>
            </label>
            <div className="flex flex-wrap gap-2 md:col-span-2">
              <button className="primary-button" disabled={saving} type="submit">
                <Icon name="check" />
                {saving ? "Saving..." : "Save Settings"}
              </button>
              <button className="secondary-button" disabled={loadingChats || !settings?.has_bot_token} onClick={() => void loadChats()} type="button">
                <Icon name="search" />
                {loadingChats ? "Loading..." : "Find Chat ID"}
              </button>
            </div>
            <div className="rounded-lg border border-brand-500/20 bg-brand-500/[0.08] px-4 py-3 text-xs leading-5 text-gray-600 dark:text-gray-300 md:col-span-2">
              Add the bot to the Telegram group, send <span className="font-semibold text-gray-900 dark:text-white">/start@CMMS_NusaBot</span> in that group, then click Find Chat ID.
            </div>
          </form>
        </Card>

        <Card>
          <CardHeader description={settings?.updated_at ? `Updated ${formatDateTime(settings.updated_at)}` : undefined} title="Test Message" />
          <div className="space-y-4 p-5">
            <TextInput
              label="Message"
              name="test_message"
              onChange={(event) => setTestMessage(event.target.value)}
              value={testMessage}
            />
            <button className="primary-button w-full" disabled={testing || !settings?.has_bot_token || !settings?.chat_id} onClick={() => void sendTestMessage()} type="button">
              <Icon name="upload" />
              {testing ? "Sending..." : "Send Test"}
            </button>
          </div>
        </Card>
      </div>

      <TablePanel
        toolbarLeft={(
          <div>
            <h2 className="text-lg font-semibold text-gray-900 dark:text-white">Telegram Chats</h2>
            <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">{chats.length} recent chats</p>
          </div>
        )}
      >
        <table className="min-w-full">
          <thead className="bg-[#6D8AF3] text-white dark:bg-[#6D8AF3]/90">
            <tr>
              <th className="min-w-56 px-5 py-3 text-left text-theme-xs font-semibold">Title</th>
              <th className="min-w-40 px-5 py-3 text-left text-theme-xs font-semibold">Type</th>
              <th className="min-w-56 px-5 py-3 text-left text-theme-xs font-semibold">Chat ID</th>
              <th className="min-w-44 px-5 py-3 text-left text-theme-xs font-semibold">Last Message</th>
              <th className="min-w-32 px-5 py-3 text-center text-theme-xs font-semibold">Action</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100 dark:divide-white/[0.05]">
            {chats.length ? chats.map((chat) => (
              <tr className="hover:bg-gray-50 dark:hover:bg-white/[0.02]" key={chat.chat_id}>
                <td className="px-5 py-4 text-theme-sm font-semibold text-gray-900 dark:text-white">{chat.title}</td>
                <td className="px-5 py-4 text-theme-sm text-gray-700 dark:text-gray-300">{chat.type}</td>
                <td className="px-5 py-4 text-theme-sm text-gray-700 dark:text-gray-300">{chat.chat_id}</td>
                <td className="px-5 py-4 text-theme-sm text-gray-500 dark:text-gray-400">{formatDateTime(chat.last_message_at)}</td>
                <td className="px-5 py-4 text-center">
                  <button className="secondary-button h-9 px-3" onClick={() => setChatId(chat.chat_id)} type="button">Use</button>
                </td>
              </tr>
            )) : (
              <tr><td className="px-5 py-8 text-center text-sm text-gray-500 dark:text-gray-400" colSpan={5}>No Telegram chats loaded.</td></tr>
            )}
          </tbody>
        </table>
      </TablePanel>
    </div>
  );
}
